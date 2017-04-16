using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace ExperimentCore
{
    public static class CommonFuncs
    {
        private const string POA_LOGON_TASK_NAME = "PfdtTest";
        private const string POA_MONITOR_TASK_NAME = "PfdtMonitor";

        public static void RegisterAtStartup()
        {
            if (!IsTaskExist(POA_LOGON_TASK_NAME))
            {
                string exePath = new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath;

                string args = string.Format("/create /sc onlogon /tn {0} /rl highest /tr \"{1}\"", POA_LOGON_TASK_NAME, exePath);
                string path = @"C:\Windows\system32\schtasks.EXE";

                //Registry.SetValue(
                //    SystemDetails.IsOs64Bit()
                //        ? "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"
                //        : "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                //    "Experiment", exePath, RegistryValueKind.String);

                Process.Start(path, args);
            }
        }

        public static void CreateMonitorTask()
        {
            if (!IsTaskExist(POA_MONITOR_TASK_NAME))
            {
                string exePath = new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath;

                string args = string.Format("/create /sc HOURLY /tn {0} /rl highest /tr \"{1}\"", POA_MONITOR_TASK_NAME, exePath);
                string path = @"C:\Windows\system32\schtasks.EXE";

                //Registry.SetValue(
                //    SystemDetails.IsOs64Bit()
                //        ? "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"
                //        : "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                //    "Experiment", exePath, RegistryValueKind.String);

                Process.Start(path, args);
            }
        }

        private static bool IsTaskExist(string taskname)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Windows\system32\schtasks.EXE";
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.Arguments = "/query /TN " + taskname;
            start.RedirectStandardOutput = true;
            // Start the process.
            using (Process process = Process.Start(start))
            {
                // Read in all the text from the process with the StreamReader.
                using (StreamReader reader = process.StandardOutput)
                {
                    string stdout = reader.ReadToEnd();
                    return stdout.Contains(taskname);
                }
            }
        }

        public static void RemoveMonitoringTask()
        {
            string args = String.Format("/Delete /tn {0} /F", POA_MONITOR_TASK_NAME);
            string path = @"C:\Windows\system32\schtasks.EXE";

            Process.Start(path, args);
        }

        public static void UnRegisterAtStartup()
        {
            //var keyName = SystemDetails.IsOs64Bit() ? 
            //                "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run" : 
            //                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            //using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
            //{
            //    if (key != null)
            //    {
            //        key.DeleteValue("Experiment");
            //    }
            //}

            string args = String.Format("/Delete /tn {0} /F", POA_LOGON_TASK_NAME);
            string path = @"C:\Windows\system32\schtasks.EXE";

            //Registry.SetValue(
            //    SystemDetails.IsOs64Bit()
            //        ? "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"
            //        : "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
            //    "Experiment", exePath, RegistryValueKind.String);

            Process.Start(path, args);
        }

        public static string SendHttpGetRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string SendHttpPostRequest(string url, string postData)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var responseString = new StreamReader(response.GetResponseStream()))
                {
                    return responseString.ReadToEnd();
                }
            }
        }
        
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
        }

        public static float PercentOf(this int part, int all)
        {
            return (part * 100) / all;
        }

        public static float PercentOf(this long part, long all)
        {
            return (part * 100) / all;
        }

        public static float PercentOfRemaining(this int part, int all)
        {
            return ((all - part) * 100) / all;
        }

        public static float PercentOfRemaining(this long part, long all)
        {
            return ((all - part) * 100) / all;
        }
    }
}