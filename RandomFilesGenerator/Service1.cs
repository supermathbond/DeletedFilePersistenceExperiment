using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace RandomFilesGenerator
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                Debug.Write("RandomFileGenerator: " + ex.ToString());
                Debug.Write("RandomFileGenerator: Terminating....");
                EventLog.WriteEntry("RandomFileGenerator: " + ex.ToString());
            };

            Configuration conf;

            try
            {
                if (ConfigurationFileExists())
                {
                    Debug.Write("RandomFileGenerator: Loading configuration.");
                    conf = Configuration.DeSerialize(AppDomain.CurrentDomain.BaseDirectory);
                }
                else
                {
                    Debug.Write("RandomFileGenerator: First run - creating configuration.");
                    conf = new Configuration()
                    {
                        CreatedFiles = new List<string>(),
                        WorkFolders = new List<string> {
                            Environment.GetEnvironmentVariable("%appdata%"),
                            Environment.GetEnvironmentVariable("%userprofile%")
                        },
                        FileCreationRandomOddsPerSecond = 120,
                        FileDeletionRandomOddsPerSecond = 120,
                        MaximumSizeOfFile = 1000000,
                        MinimumSizeOfFile = 1024,
                        MinimumFreeSpace = 1000000

                    };
                    Configuration.Serialize(conf, AppDomain.CurrentDomain.BaseDirectory);
                }

                new Thread((x) => new DeleterCreatorSimulator(conf).Run()).Start();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                try
                {
                    Debug.Write(ex);
                }
                catch { }
            }
        }

        protected override void OnStop()
        {
        }


        private static bool ConfigurationFileExists()
        {
            return File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Configuration.FILE_NAME));
        }
    }
}
