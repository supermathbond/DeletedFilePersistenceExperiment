using System;
using System.IO;
using System.Threading;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]
namespace TezaExperiment
{
    class Program
    {
        private const string DB_OFFLINE_NAME = "tempDb.xml";

        static void Main(string[] args)
        {
            ILog log = LogManager.GetLogger("MyLog");

            try
            {
                SelfUpdateHandler updateHandler = new SelfUpdateHandler(log);
                updateHandler.CleanUpdateFileIfNeeded(args);

                Configuration conf;
                DAL dal;

                if (ConfigurationFileExists())
                {
                    log.Info("Loading configuration.");
                    conf = Configuration.DeSerialize();
                    dal = new DAL(GetDbOfflineFilePath(), conf.Id);
                }
                else
                {
                    dal = new DAL(GetDbOfflineFilePath());

                    // Exit only when DB is down and no configuration found (meaning - first run).
                    if (dal.IsDbOffline(true))
                        throw new Exception("### DB is Offline! ###");

                    log.Info("First run - creating configuration.");
                    conf = new Configuration { Id = dal.GenerateClientId() };
                    Configuration.Serialize(conf);
                }
                
                log.Info("Got ID => " + conf.Id);

                CommonFuncs.RegisterAtStartup();
                new MissionsManager(log, updateHandler, dal, conf).Run();

                // Block thread.
                WaitHandle waitHandle = new AutoResetEvent(false);
                waitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
