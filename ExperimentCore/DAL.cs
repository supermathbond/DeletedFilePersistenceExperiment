using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExperimentCore
{
    public class MissionDetails
    {
        public int MissionId { get; set; }

        public MissionType TypeOfMission { get; set; }

        public DateTime MissionTimeCreation { get; set; }

        public string TempFolder { get; set; }

        public string MoreDetails { get; set; }

        public DateTime DateOfAttack { get; set; }

        public int TimeGap { get; set; }

        public byte[] ClusterData { get; set; }

        public int NumOfClustersToCreate { get; set; }

        public override string ToString()
        {
            return string.Format("Id = {0}, type = {1}, Date of attack = {2}, Temp folder = {3}, NumOfClustersToCreate = {4}", MissionId, TypeOfMission, DateOfAttack, TempFolder, NumOfClustersToCreate);
        }
    }

    public enum MissionType
    {
        Experiment = 1,
        Update = 2,
        Delete = 3
    };

    public enum AgentStatus
    {
        Idle = 1,
        CreatingExperiment = 2,
        IdleInTheMiddleOfExperiment = 3,
        ExperimentCreatedAndWaitingToStart = 4,
        ScanningHd = 5,
        Updating = 6,
        CommitingSuicide = 7
    }

    public class DAL
    {
        public const string WEBSITE_ADDRESS = "http://130.211.98.43/dfpt";
        public const int KEEPALIVE_PERIOD = 60*1000;
        public const int DUMP_LOCAL_DB_PERIOD = 500000;

        private readonly string _offlineFilePath;
        private readonly object _fileLocker = new object();

        private readonly Timer _connectionTimer;
        private Timer _keepAliveTimer = null;
        private AgentStatus _agentStatus;
      
        public int Id { get; private set; }

        public int HoursDiff { get; set; }

        public DAL(string offlineFilePath, int id = -1, int hoursDiff = 0)
        {
            _agentStatus = AgentStatus.Idle;
            _offlineFilePath = offlineFilePath;
            Id = id;
            HoursDiff = hoursDiff;

            _connectionTimer = new Timer((x) => DumpTempDbToServer(), null, DUMP_LOCAL_DB_PERIOD, DUMP_LOCAL_DB_PERIOD);
            if (id != -1)
            {
                SubscriseCurrentVersion(id);
                _keepAliveTimer = new Timer((x) => UpdateKeepAlive(), null, 0, KEEPALIVE_PERIOD);
            }
        }

        /// <summary>
        /// Try connecting back to server and update in DB all links which are in the local DB file.
        /// </summary>
        private void DumpTempDbToServer()
        {
            try
            {
                _connectionTimer.Change(Timeout.Infinite, Timeout.Infinite);

                lock (_fileLocker)
                {
                    if (File.Exists(_offlineFilePath) && CheckDbAvailability())
                    {
                        bool wasNewFileFilled = false;
                        using (StreamReader reader = new StreamReader(_offlineFilePath, Encoding.UTF8))
                        {
                            using (StreamWriter writer = new StreamWriter(_offlineFilePath + ".temp", false, Encoding.UTF8))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string line = reader.ReadLine();
                                    try
                                    {
                                        InsertDataToDb(line);
                                    }
                                    catch
                                    {
                                        writer.WriteLine(line);
                                        wasNewFileFilled = true;
                                    }

                                }
                            }
                        }

                        File.Delete(_offlineFilePath);
                        if (wasNewFileFilled)
                            File.Move(_offlineFilePath + ".temp", _offlineFilePath);
                        else
                            File.Delete(_offlineFilePath + ".temp");
                    }
                }
            }
            catch
            {
                _connectionTimer.Change(DUMP_LOCAL_DB_PERIOD, DUMP_LOCAL_DB_PERIOD);
            }
        }

        private void AddToInternalFile(string url)
        {
            lock (_fileLocker)
            {
                bool shouldAppend = !File.Exists(_offlineFilePath);
                using (StreamWriter writer = new StreamWriter(_offlineFilePath, shouldAppend, Encoding.UTF8))
                {
                    writer.WriteLine(url);
                }
            }
        }

        private static void InsertDataToDb(string url)
        {
            Debug.Write(url);
            string response = string.Empty;
            if (url.StartsWith("POST:"))
            {
                string[] portions = url.Replace("POST:", "").Split('?');
                response = CommonFuncs.SendHttpPostRequest(portions[0], portions[1]);
            }
            else
            {
                response = CommonFuncs.SendHttpGetRequest(url);
            }

            if (response.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Any(
                part => part.StartsWith("ERROR=") && !part.StartsWith("ERROR=Duplicate entry")))
            {
                throw new Exception("### DB is Offline! ###");
            }
        }

        private bool CheckDbAvailability()
        {
            string url = string.Format("{0}/CheckConnection.php", WEBSITE_ADDRESS);
            Debug.Write(url);
            try
            {
                string response = CommonFuncs.SendHttpGetRequest(url);

                foreach (var part in response.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (part.StartsWith("CONNECTION="))
                    {
                        return "OPEN" == part.Replace("CONNECTION=", "");
                    }
                }
                return false;
            }
            catch 
            {
                return false;
            }
        }

        private void SaveToDb(string url)
        {            
            try
            {
                InsertDataToDb(url);
            }
            catch
            {
                AddToInternalFile(url);
            }
        }

        /// <summary>
        /// Checks if remote DB's available.
        /// </summary>
        /// <returns> Is remote DB's available. </returns>
        public bool IsDbOffline()
        {
            return !CheckDbAvailability();
        }

        /// <summary>
        /// Generate new client ID.
        /// </summary>
        /// <returns></returns>
        /// <remarks> If remote DB isn't connected an exception will be thrown. </remarks>
        public int GenerateClientId()
        {
            SystemDetails details = new SystemDetails();
            
            string url = string.Format(
                "{0}/GenerateClientId.php?Cpu={1}&Ram={2}&FreeSpace={3}&DiskSize={4}&OS={5}&ServicePack={6}&is64Bit={7}&HostName={8}&UserName={9}&Version={10}", WEBSITE_ADDRESS,
                details.CPU, details.RamMemorySIze, details.PartitionFreeSpace, details.PartitionSize, details.OS, details.SP, details.Is64Bit ? "1" : "0", details.HostName, details.UserName,
                typeof(DAL).Assembly.GetName().Version);
            Debug.Write(url);
            string response = CommonFuncs.SendHttpGetRequest(url);
            Debug.Write(response);
            foreach (var part in response.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.StartsWith("ID="))
                {
                    Id = int.Parse(part.Replace("ID=", ""));
                    _keepAliveTimer = new Timer((x) => UpdateKeepAlive(), null, 0, KEEPALIVE_PERIOD);
                    SubscriseCurrentVersion(Id);
                    break;
                }
                else if (part.StartsWith("ERROR=") && !part.StartsWith("ERROR=Duplicate entry"))
                {
                    break;
                }
            }
            return Id;
        }

        public void SubscriseCurrentVersion(int id)
        {
            string url = string.Format(
                "{0}/SubscriseCurrentVersion.php?Id={1}&Version={2}&CurrentFolder={3}", WEBSITE_ADDRESS, id, typeof(DAL).Assembly.GetName().Version,
                AppDomain.CurrentDomain.BaseDirectory.Replace(" ", "%20").Replace("\\", "\\\\"));
            Debug.Write(url);
            string response = CommonFuncs.SendHttpGetRequest(url);
            
            // For now don't need that.
            foreach (var part in response.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.StartsWith("ERROR=") && !part.StartsWith("ERROR=Duplicate entry"))
                {
                    break;
                }
            }
        }

        public void UpdateKeepAlive()
        {
            try
            {
                _keepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                string url = string.Format("{0}/UpdateKeepAlive.php?Id={1}&StatusId={2}&Date={3}",
                    WEBSITE_ADDRESS, Id, (int)_agentStatus,
                    DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss").Replace(" ", "%20"));
                Debug.Write(url);
                CommonFuncs.SendHttpGetRequest(url);
            }
            catch
            {
                // Nothing to do... 
            }
            finally
            {
                _keepAliveTimer.Change(KEEPALIVE_PERIOD, KEEPALIVE_PERIOD);
            }
        }

        public void SetMissionAsFinished(int missionId)
        {
            string url = string.Format("{0}/FinishMission.php?MissionId={1}&ClientId={2}", WEBSITE_ADDRESS, missionId, Id);
            
            SaveToDb(url);
        }

        internal MissionDetails GetLastUpdate(DateTime lastCheckInDb)
        {
            try
            {
                string url = string.Format("{0}/getLastUpdate.php?clientId={1}", WEBSITE_ADDRESS, Id);

                if (lastCheckInDb != default(DateTime))
                {
                    url += "&from=" + lastCheckInDb.ToString("MM-dd-yyyy HH:mm:ss").Replace(" ", "%20");
                }
                Debug.Write(url);

                string response = CommonFuncs.SendHttpGetRequest(url);
                if (!response.Contains(','))
                    return null;
                var rowArr = response.Split(',');
                return new MissionDetails
                {
                    MissionId = Convert.ToInt32(rowArr[0]),
                    TypeOfMission = (MissionType)Enum.Parse(typeof(MissionType), Convert.ToInt32(rowArr[1]).ToString()),
                    MissionTimeCreation = DateTime.ParseExact(Convert.ToString(rowArr[2]), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    TempFolder = Convert.ToString(rowArr[3]),
                    MoreDetails = string.IsNullOrEmpty(rowArr[4]) ? "" : Convert.ToString(rowArr[4]),
                    DateOfAttack = string.IsNullOrEmpty(rowArr[5]) ? DateTime.MinValue : DateTime.ParseExact(rowArr[5], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    TimeGap = string.IsNullOrEmpty(rowArr[6]) ? -1 : Convert.ToInt32(rowArr[6]),
                    ClusterData = string.IsNullOrEmpty(rowArr[7]) ? new byte[0] : Convert.FromBase64String(Convert.ToString(rowArr[7])),
                    NumOfClustersToCreate = string.IsNullOrEmpty(rowArr[8]) ? -1 : Convert.ToInt32(rowArr[8])
                };
            }
            catch (Exception ex)
            {
                Debug.Write("Error: " + ex);
                return null;
            }
        }

        public void AddExperimentResult(int missionId, LeftoverChunks[] chunks)
        {
            string currentDate = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss").Replace(" ", "%20");
            int howManyAlive = chunks.Count(x => x.PercentageCompleted > 1.0);
            string summaryUrl = string.Format("{0}/AddExperimentResultsSummary.php?MissionId={1}&DateTime={2}&HowManyAlive={3}",
                        WEBSITE_ADDRESS, missionId, currentDate, howManyAlive);
            SaveToDb(summaryUrl);

            StringBuilder data = new StringBuilder();
            for (int i = 0; i < chunks.Length; i++)
            {
                data.Append(chunks[i].IsAllocated ? "1," : "0,");
                data.Append(chunks[i].Lcn.ToString() + ",");
                data.Append(Math.Round(chunks[i].PercentageCompleted,2).ToString() +";");
                if(data.Length > 8 * 1024 || i + 1 == chunks.Length) // 8KB
                {
                    string url = string.Format("POST:{0}/AddExperimentResults.php?MissionId={1}&DateTime={2}&parsedData={3}",
                        WEBSITE_ADDRESS, missionId, currentDate, data);
                    SaveToDb(url);
                    data = new StringBuilder();
                }
            }
        }

        public void UpdateId(int id)
        {
            if (Id == -1)
            {
                Id = id;
                _keepAliveTimer = new Timer((x) => UpdateKeepAlive(), null, 0, KEEPALIVE_PERIOD);
            }
            else
            {
                throw new NotSupportedException("ID should not be changes after it being initialized!");
            }
        }

        public IEnumerable<MissionDetails> GetMission(DateTime? from)
        {
            try
            {
                string url = string.Format("{0}/selectMissions.php?clientId={1}", WEBSITE_ADDRESS, Id);
            
                if (from.HasValue && from != default(DateTime))
                {
                    url += "&from=" + from.Value.ToString("MM-dd-yyyy HH:mm:ss").Replace(" ", "%20");
                }
                Debug.Write(url);

                string response = CommonFuncs.SendHttpGetRequest(url);
                string[] rows = response.Split(new []{"<br />"}, StringSplitOptions.RemoveEmptyEntries);
                var items = rows.Select(row => new { rowArr = row.Split(',')}).Select(@t => new MissionDetails
                {
                    MissionId = Convert.ToInt32(@t.rowArr[0]),
                    TypeOfMission = (MissionType) Enum.Parse(typeof (MissionType), Convert.ToInt32(@t.rowArr[1]).ToString()),
                    MissionTimeCreation = DateTime.ParseExact(Convert.ToString(@t.rowArr[2]), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    TempFolder = Convert.ToString(@t.rowArr[3]),
                    MoreDetails = string.IsNullOrEmpty(@t.rowArr[4])? "":Convert.ToString(@t.rowArr[4]),
                    DateOfAttack = string.IsNullOrEmpty(@t.rowArr[5]) ? DateTime.MinValue : DateTime.ParseExact(@t.rowArr[5], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    TimeGap = string.IsNullOrEmpty(@t.rowArr[6]) ? -1 : Convert.ToInt32(@t.rowArr[6]),
                    ClusterData = string.IsNullOrEmpty(@t.rowArr[7]) ? new byte[0] : Convert.FromBase64String(Convert.ToString(@t.rowArr[7])),
                    NumOfClustersToCreate = string.IsNullOrEmpty(@t.rowArr[8]) ? -1 : Convert.ToInt32(@t.rowArr[8])
                });
                return items;
            }
            catch(Exception ex)
            {
                Debug.Write("Error: " + ex);
                return new List<MissionDetails>();
            }
        }
        
        public void WriteError(string exception, int retries = 3)
        {
            string url = string.Format(
                    "{0}/InsertError.php?clientId={1}&error={2}&date={3}", WEBSITE_ADDRESS, Id, exception.Replace("\n", " ").Replace("\r", "").Replace(" ", "%20").Replace("'", "\""), DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss").Replace(" ", "%20"));
            SaveToDb(url);
        }

        public void SetAgentStatus(AgentStatus status)
        {
            _agentStatus = status;
            UpdateKeepAlive();
        }

        public int GetHoursDiffFromServer()
        {
            string url = "{0}/getDateOnServer.php";
            string response = CommonFuncs.SendHttpGetRequest(url);

            DateTime now = DateTime.Now;
            DateTime nowOnServer = DateTime.ParseExact(response, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            HoursDiff = (int)(now - nowOnServer).TotalHours;
            return HoursDiff;
        }
    }
}