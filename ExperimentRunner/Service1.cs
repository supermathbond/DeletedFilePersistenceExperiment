using System;
using System.Diagnostics;
using System.ServiceProcess;
using ExperimentCore;

namespace ExperimentRunner
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
                Debug.Write(ex.ToString());
                Debug.Write("Terminating....");
                EventLog.WriteEntry(ex.ToString());
            };

            Manager expManager = new Manager();
            expManager.Start(this.Stop);
        }

        protected override void OnStop()
        {
            Debug.Write("Killing service from serviceBase");
        }
    }
}
