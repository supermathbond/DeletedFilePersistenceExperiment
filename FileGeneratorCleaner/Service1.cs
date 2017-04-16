using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace FileGeneratorCleaner
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
                    foreach (string file in conf.CreatedFiles)
                    {
                        File.Delete(file);
                    }
                }

                Environment.Exit(0);
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
