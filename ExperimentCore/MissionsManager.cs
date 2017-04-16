using System;
using System.IO;
using System.Threading;
using log4net;
using System.Linq;

namespace ExperimentCore
{
    public class MissionsManager
    {
        private const int TIME_FOR_CHECKING_DB_AGAIN = 1000 * 60;
        private const int TIME_FOR_CHECKING_DB_AGAIN_FOR_UPDATE_ONLY = 1000 * 60 * 10;

        private readonly Timer _mainTimer;
        private readonly Timer _deleteFileAndStartExperimentTimer;
        private readonly Timer _filesLocatorTimer;
        private readonly Timer _updateTimer;

        private FilesLocatorNG _locator;

        private readonly DAL _dal;
        private readonly Configuration _conf;
        private readonly SelfUpdateHandler _updateHandler;
        private readonly ILog _log;

        private readonly Action _closeAppMethod;

        readonly Random _rand = new Random();

        public MissionsManager(ILog logger, SelfUpdateHandler updateHandler, DAL dal, Configuration conf, Action closeAppMethod)
        {
            _log = logger;
            _updateHandler = updateHandler;
            _dal = dal;
            _conf = conf;
            _closeAppMethod = closeAppMethod;

            _mainTimer = new Timer(GetMission, null, Timeout.Infinite, Timeout.Infinite);
            _filesLocatorTimer = new Timer(CheckHd, null, Timeout.Infinite, Timeout.Infinite);
            _deleteFileAndStartExperimentTimer = new Timer(StartExperiment, null, Timeout.Infinite, Timeout.Infinite);
            _updateTimer = new Timer(UpdateInMiddleOfExperiment, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Run()
        {
            _log.Info("Running Mission Manager");
            // If no mission is in progress.
            if (!_conf.InTheMiddleOfMission || _conf.DetailsOfAttack == null)
            {
                _log.Info("No recent missions found.");
                _mainTimer.Change(0, TIME_FOR_CHECKING_DB_AGAIN);
            }
            else
            {
                _updateTimer.Change(TIME_FOR_CHECKING_DB_AGAIN_FOR_UPDATE_ONLY, TIME_FOR_CHECKING_DB_AGAIN_FOR_UPDATE_ONLY);

                _log.Info("Mission already in progress.");
                if (_conf.DetailsOfAttack.InTheMiddleOfScan)
                {
                    _log.Info("Checking if files still exist.");
                    _locator = new FilesLocatorNG(_conf.DetailsOfAttack.FileLocationData, _conf.DetailsOfAttack.DataToSearchFor, _log);
                    CheckHd(null);
                }
                else
                {
                    if (_conf.DetailsOfAttack.WhenToStart < DateTime.Now)
                    {
                        _deleteFileAndStartExperimentTimer.Change(0, Timeout.Infinite);
                    }
                    else
                    {
                        _deleteFileAndStartExperimentTimer.Change((long)(_conf.DetailsOfAttack.WhenToStart - DateTime.Now).TotalMilliseconds, Timeout.Infinite);
                        _dal.SetAgentStatus(AgentStatus.ExperimentCreatedAndWaitingToStart);
                    }
                }
            }
        }

        private void CheckHd(object state)
        {
            try
            {
                _dal.SetAgentStatus(AgentStatus.ScanningHd);
                _filesLocatorTimer.Change(Timeout.Infinite, Timeout.Infinite);

                _log.Info("Searching for malicious file.");
                LeftoverChunks[] chunks = _locator.GetNumberOfExistingMaliciousParts();
                AddExperimentResultAndFinishItIfNeeded(chunks);
            }
            catch (Exception ex)
            {
                try
                {
                    _log.Error(ex);
                    _dal.WriteError(ex.ToString());
                }
                catch {}
                _filesLocatorTimer.Change(_conf.DetailsOfAttack.TimeGap, _conf.DetailsOfAttack.TimeGap);
            }
        }

        private void StartExperiment(object state)
        {
            _dal.SetAgentStatus(AgentStatus.CreatingExperiment);
            _conf.DetailsOfAttack.FileLocationData = NtfsHelper.GetFileMap(_conf.DetailsOfAttack.FilePath);
            _locator = new FilesLocatorNG(_conf.DetailsOfAttack.FileLocationData, _conf.DetailsOfAttack.DataToSearchFor, _log);
            
            try
            {
                File.Delete(_conf.DetailsOfAttack.FilePath);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _dal.WriteError("Error while deleting incriminating file! " + ex.ToString());
                throw;
            }

            _conf.DetailsOfAttack.InTheMiddleOfScan = true;
            Configuration.Serialize(_conf, AppDomain.CurrentDomain.BaseDirectory);

            CheckHd(null);
        }

        private void AddExperimentResultAndFinishItIfNeeded(LeftoverChunks[] chunks)
        {
            _log.Info("adding experiment result");
            _dal.AddExperimentResult(_conf.DetailsOfAttack.MissionId, chunks);

            if (chunks.All(x => x.PercentageCompleted < 1.0)) // No More than 40 bytes...
            {
                _dal.SetMissionAsFinished(_conf.DetailsOfAttack.MissionId);

                _locator = null;

                _conf.DetailsOfAttack = new AttackDetails();
                _conf.InTheMiddleOfMission = false;
                Configuration.Serialize(_conf, AppDomain.CurrentDomain.BaseDirectory);
                _dal.SetAgentStatus(AgentStatus.Idle);
                
                _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _mainTimer.Change(TIME_FOR_CHECKING_DB_AGAIN, TIME_FOR_CHECKING_DB_AGAIN);
                _log.Info("Experiment finished");
                foreach (var chunk in chunks)
                    _log.Debug(string.Format("LCN: {0}, Percent: {1}, IsAllocated:{2}", chunk.Lcn, chunk.PercentageCompleted, chunk.IsAllocated));
            }
            else
            {
                _filesLocatorTimer.Change(_conf.DetailsOfAttack.TimeGap, _conf.DetailsOfAttack.TimeGap);

                _dal.SetAgentStatus(AgentStatus.IdleInTheMiddleOfExperiment);
            }
        }

        private void GetMission(object state)
        {
            try
            {
                _mainTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _dal.SetAgentStatus(AgentStatus.Idle);

                if (_dal.IsDbOffline())
                {
                    _log.Info("### DB is Offline! ###");
                    _mainTimer.Change(TIME_FOR_CHECKING_DB_AGAIN, TIME_FOR_CHECKING_DB_AGAIN);
                    return;
                }

                _log.Debug("Checking new missions: " + DateTime.Now);
                foreach (var mission in _dal.GetMission(_conf.LastCheckInDb))
                {
                    _log.Debug("Got mission: " + mission);
                    _conf.LastCheckInDb = mission.MissionTimeCreation;
                    switch (mission.TypeOfMission)
                    {
                        case MissionType.Experiment: CreateNewExperiment(mission); return;
                        case MissionType.Update: UpdateAgentAndRestart(mission); return;
                        case MissionType.Delete: CommitSuicide(mission); return;
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _log.Error(ex);
                    _dal.WriteError(ex.ToString());
                }
                catch {}
            }

            _mainTimer.Change(TIME_FOR_CHECKING_DB_AGAIN, TIME_FOR_CHECKING_DB_AGAIN);
        }

        private void CreateNewExperiment(MissionDetails mission)
        {
            _log.Info("Creating experiment: " + mission);
            _dal.SetAgentStatus(AgentStatus.CreatingExperiment);

            Directory.CreateDirectory(mission.TempFolder);

            string filePath = Path.Combine(mission.TempFolder, Guid.NewGuid().ToString() + ".tmp");
            ExperimentCreator expCreator = new ExperimentCreator();
            expCreator.CreateExperiment(filePath, mission.ClusterData, mission.NumOfClustersToCreate);

            FillConfigurationWithMissionDetails(mission, filePath);

            _dal.SetAgentStatus(AgentStatus.ExperimentCreatedAndWaitingToStart);
            if(_conf.DetailsOfAttack.WhenToStart < DateTime.Now)
                _deleteFileAndStartExperimentTimer.Change(0, Timeout.Infinite);
            else
                _deleteFileAndStartExperimentTimer.Change((long)(_conf.DetailsOfAttack.WhenToStart - DateTime.Now).TotalMilliseconds, Timeout.Infinite);

            _updateTimer.Change(TIME_FOR_CHECKING_DB_AGAIN_FOR_UPDATE_ONLY, TIME_FOR_CHECKING_DB_AGAIN_FOR_UPDATE_ONLY);
        }

        private void FillConfigurationWithMissionDetails(MissionDetails mission, string filePath)
        {
            if (_conf.DetailsOfAttack == null)
                _conf.DetailsOfAttack = new AttackDetails();

            _conf.DetailsOfAttack.FilePath = filePath;
            _conf.DetailsOfAttack.DataToSearchFor = mission.ClusterData;
            _conf.DetailsOfAttack.WhenToStart = mission.DateOfAttack;
            _conf.DetailsOfAttack.MissionId = mission.MissionId;
            _conf.DetailsOfAttack.TimeGap = mission.TimeGap;
            _conf.InTheMiddleOfMission = true;
            Configuration.Serialize(_conf, AppDomain.CurrentDomain.BaseDirectory);
        }

        private void CommitSuicide(MissionDetails mission)
        {
            CommonFuncs.UnRegisterAtStartup();
            CommonFuncs.RemoveMonitoringTask();

            _log.Info("Going to delete app.");
            _dal.SetAgentStatus(AgentStatus.CommitingSuicide);
            Thread.Sleep(5000);
            string folder = mission.MoreDetails.ToLower() == "--deletetempfolder" ? mission.TempFolder : "";
            new SelfDeleteHandler(_log, folder).DeleteAppInSeperateProcess();

            try
            {
                _dal.SetMissionAsFinished(mission.MissionId);
                _closeAppMethod();
            }
            catch (Exception e)
            {
                _log.Error("error while killing: " + e);
                _dal.WriteError("error while killing: " +  e);
            }
        }

        private void UpdateInMiddleOfExperiment(object state)
        {
            try
            {
                _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                MissionDetails misssion = _dal.GetLastUpdate(_conf.LastCheckInDb);

                if (misssion != null)
                    UpdateAgentAndRestart(misssion);
            }
            catch(Exception ex)
            {
                _log.Error("NOOOOO!: " + ex);
            }
            finally
            {
                _updateTimer.Change(TIME_FOR_CHECKING_DB_AGAIN_FOR_UPDATE_ONLY, TIME_FOR_CHECKING_DB_AGAIN_FOR_UPDATE_ONLY);
            }
        }

        private void UpdateAgentAndRestart(MissionDetails mission)
        {
            _dal.SetAgentStatus(AgentStatus.Updating);
            _log.Info("Going to update app." + mission);
            _updateHandler.UpdateAppInSeperateProcess(mission.MoreDetails);
            _dal.SetMissionAsFinished(mission.MissionId);
            Thread.Sleep(5000);
            Configuration.Serialize(_conf, AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                _closeAppMethod();
            }
            catch (Exception e)
            {
                _log.Error("error while killing: " + e);
                _dal.WriteError("error while killing: " + e);
            }
        }
    }
}