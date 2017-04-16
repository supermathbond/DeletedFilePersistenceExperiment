using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace ExperimentCore
{
    public class Manager
    {
        private const string DB_OFFLINE_NAME = "tempDb.db";

        private Action _closeAppMethod;

        private const int RECONNECT_TIME = 360000;

        private ILog _log;
        private SelfUpdateHandler _updateHandler;

        public void Start(Action closeAppMethod)
        {
            _closeAppMethod = closeAppMethod;

            _log = LogManager.GetLogger("MyLog");
            _updateHandler = new SelfUpdateHandler(_log);

            new Thread(RunManager).Start();
        }

        private void RunManager(object obj)
        {
            Configuration conf;
            DAL dal = null;

            while (true)
            {
                try
                {
                    if (ConfigurationFileExists())
                    {
                        _log.Info("Loading configuration.");
                        conf = Configuration.DeSerialize(AppDomain.CurrentDomain.BaseDirectory);
                        dal = new DAL(GetDbOfflineFilePath(), conf.Id);
                    }
                    else
                    {
                        dal = new DAL(GetDbOfflineFilePath());

                        // Exit only when DB is down and no configuration found (meaning - first run).
                        if (dal.IsDbOffline())
                            throw new Exception("### DB is Offline! ###");

                        _log.Info("First run - creating configuration.");
                        conf = new Configuration { Id = dal.GenerateClientId() };
                        Configuration.Serialize(conf, AppDomain.CurrentDomain.BaseDirectory);
                    }

                    _log.Info("Got ID => " + conf.Id);
                    new MissionsManager(_log, _updateHandler, dal, conf, _closeAppMethod).Run();
                    break;
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                    try
                    {
                        _log.Error(ex);
                        dal.WriteError(ex.ToString());
                    }
                    catch { }
                    Thread.Sleep(RECONNECT_TIME);
                }
            }
        }

        private static string GetDbOfflineFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DB_OFFLINE_NAME);
        }

        private static bool ConfigurationFileExists()
        {
            return File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Configuration.FILE_NAME));
        }
    }
}
