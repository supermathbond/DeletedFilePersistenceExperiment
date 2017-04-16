using System;
using System.Diagnostics;
using System.IO;
using log4net;

namespace ExperimentCore
{
    public class SelfDeleteHandler
    {
        private readonly ILog _log;

        private readonly string _tempFolder;

        private const string BAT_DELETE_FILE = "delete.bat";

        public SelfDeleteHandler(ILog logger, string tempFolder)
        {
            _log = logger;
            _tempFolder = tempFolder;
        }

        private void CreateSelfDeleteBatch(string localFilename, string appFilename)
        {
            string baseFolder = Path.GetDirectoryName(localFilename);
            bool is64 = SystemDetails.IsOs64Bit();
            using (StreamWriter writer = new StreamWriter(localFilename))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine(":CHECK");
                writer.WriteLine("tasklist /fi \"PID eq " + Process.GetCurrentProcess().Id+ "\" | find \"" + Process.GetCurrentProcess().Id + "\">NUL");
                writer.WriteLine("if \"%ERRORLEVEL%\"==\"0\" goto WasteTime");
                writer.WriteLine("goto Delete");
                writer.WriteLine(":WasteTime");
                writer.WriteLine("timeout /t 1");
                writer.WriteLine("goto CHECK");
                writer.WriteLine(":Delete");

                //string bitsVersion = is64 ? "64" : "";
                writer.WriteLine(@"%SystemRoot%\Microsoft.NET\Framework{0}\v2.0.50727\installutil.exe -u {1}", "", appFilename);
                writer.WriteLine("cd \"" + baseFolder + "\"");

                writer.WriteLine("del Conf.xml");
                writer.WriteLine("del Conf.bak.xml");
                writer.WriteLine("del log4net.dll");
                writer.WriteLine("del log4net.xml");
                writer.WriteLine("del mylogfile*");
                writer.WriteLine("del TezaExperiment.dll");
                writer.WriteLine("del TezaExperiment.dll.config");
                writer.WriteLine("del TezaExperiment.pdb");
                //writer.WriteLine("del InstallUtil.InstallLog");
                writer.WriteLine("del \"" + appFilename + "\"");
                //writer.WriteLine("del \"" + appFilename.Replace(".exe", ".InstallLog") + "\"");
                //writer.WriteLine("del \"" + appFilename.Replace(".exe", ".InstallState") + "\"");
                writer.WriteLine("del \"" + appFilename.Replace(".exe", ".pdb") + "\"");
                writer.WriteLine("del \"" + appFilename + ".config" + "\"");

                // Bat deletes itself
                
                if (!string.IsNullOrEmpty(_tempFolder) && localFilename.Contains(_tempFolder))
                    writer.WriteLine("rmdir /Q /S \"" + _tempFolder + "\"");
                else
                    writer.WriteLine("del /F \"" + localFilename + "\"");
            }
        }

        public void DeleteAppInSeperateProcess()
        {
            try
            {
                string batFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BAT_DELETE_FILE);
                string appFilename = Process.GetCurrentProcess().MainModule.FileName;
                CreateSelfDeleteBatch(batFilename, appFilename);
            
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = batFilename;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true; // <- key line
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process process = Process.Start(psi);
            }
            catch (Exception ex)
            {
                _log.Error("Could not commit suicide: " + ex);
            }
        }
    }
}