using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using log4net;

namespace ExperimentCore
{
    public class SelfUpdateHandler
    {
        private readonly ILog _log;

        private const string BAT_UPDATE_FILE = "update.bat";

        private const string UPDATE_SCHEDULE_TASK_NAME = "PoaTestUpdate";

        public SelfUpdateHandler(ILog logger)
        {
            _log = logger;

            // Remove schedule task if exists...
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = @"C:\Windows\system32\schtasks.EXE";
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = false;
            psi.Arguments = string.Format("/Delete /tn {0} /F", UPDATE_SCHEDULE_TASK_NAME);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true; // <- key line
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            Process process = Process.Start(psi);
        }

        private void CreateSelfUpdateBatch(string localFilename, string dllPath, string localUpdateFilename)
        {
            using (StreamWriter writer = new StreamWriter(localFilename))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine(":CHECK");
                writer.WriteLine("tasklist /fi \"PID eq " + Process.GetCurrentProcess().Id + "\" | find \"" + Process.GetCurrentProcess().Id + "\">NUL");
                writer.WriteLine("if \"%ERRORLEVEL%\"==\"0\" goto WasteTime");
                writer.WriteLine("goto Update");
                writer.WriteLine(":WasteTime");
                writer.WriteLine("timeout /t 1");
                writer.WriteLine("goto CHECK");
                writer.WriteLine(":Update");
                writer.WriteLine("del \"" + dllPath + "\"");
                writer.WriteLine("move /Y \"" + localUpdateFilename + "\" \"" + dllPath + "\"");
                writer.WriteLine("sc start ExperimentRunner");
                writer.WriteLine("del /F \"" + localFilename + "\"");
            }
        }

        private static void CreateUpdateScheduleTask(string appPath)
        {
            DateTime d = DateTime.Now.AddMinutes(2);

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = @"C:\Windows\system32\schtasks.EXE";
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = false;
            psi.Arguments = string.Format("/create /sc once /tn {0} /rl highest /tr \"{1}\" /ST {2}", UPDATE_SCHEDULE_TASK_NAME,
                appPath, d.ToString("HH:mm"));
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true; // <- key line
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(psi);
        }

        public void UpdateAppInSeperateProcess(string link)
        {
            string localUpdateFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.dll");
            string batFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BAT_UPDATE_FILE);
            CreateSelfUpdateBatch(batFilename, Assembly.GetExecutingAssembly().Location, localUpdateFilename);
            CreateUpdateScheduleTask(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(link, localUpdateFilename);
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = batFilename;
            psi.UseShellExecute = true;
            psi.CreateNoWindow = true; // <- key line
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(psi);
        }
    }
}
