using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace ExperimentCore
{
    public class SystemDetails
    {
        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public extern static IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public extern static IntPtr GetProcAddress(IntPtr hwnd, string procedureName);

        private delegate bool IsWow64ProcessDelegate([In] IntPtr handle, [Out] out bool isWow64Process);

        public static bool IsOs64Bit()
        {
            return IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor());
        }

        private static IsWow64ProcessDelegate GetIsWow64ProcessDelegate()
        {
            IntPtr handle = LoadLibrary("kernel32");

            if (handle != IntPtr.Zero)
            {
                IntPtr fnPtr = GetProcAddress(handle, "IsWow64Process");

                if (fnPtr != IntPtr.Zero)
                {
                    return (IsWow64ProcessDelegate)Marshal.GetDelegateForFunctionPointer((IntPtr)fnPtr, typeof(IsWow64ProcessDelegate));
                }
            }

            return null;
        }

        private static bool Is32BitProcessOn64BitProcessor()
        {
            IsWow64ProcessDelegate fnDelegate = GetIsWow64ProcessDelegate();

            if (fnDelegate == null)
            {
                return false;
            }

            bool isWow64;
            bool retVal = fnDelegate.Invoke(Process.GetCurrentProcess().Handle, out isWow64);

            if (retVal == false)
            {
                return false;
            }

            return isWow64;
        }

        public long PartitionFreeSpace { get; set; }

        public long PartitionSize { get; set; }

        public string OS { get; set; }

        public string SP { get; set; }

        public string CPU { get; set; }

        public bool Is64Bit { get; set; }

        public double RamMemorySIze { get; set; }

        public string HostName { get; set; }

        public string UserName { get; set; }
        
        public SystemDetails()
        {
            DriveInfo drive = new DriveInfo("C:");
            PartitionFreeSpace = drive.TotalFreeSpace;
            PartitionSize = drive.TotalSize;

            try
            {
                using (var query = new ManagementObjectSearcher("select * from Win32_OperatingSystem"))
                {
                    query.Options.Timeout = new TimeSpan(0, 0, 30);
                    var wmi = query.Get().Cast<ManagementObject>().First();
                    OS = ((string)wmi["Caption"]).Trim();
                    SP = ((ushort)wmi["ServicePackMajorVersion"]).ToString().Trim() + "." +
                         ((ushort)wmi["ServicePackMinorVersion"]).ToString().Trim();
                }
            }
            catch { }

            try
            {
                using (var query = new ManagementObjectSearcher("select * from Win32_Processor"))
                {
                    query.Options.Timeout = new TimeSpan(0, 0, 30);
                    var wmi = query.Get().Cast<ManagementObject>().First();
                    CPU = ((string)wmi["Name"]).Trim();
                }
            }
            catch { }

            Is64Bit = IsOs64Bit();

            try
            {
                using (ManagementObjectSearcher query =
                    new ManagementObjectSearcher("Select * From Win32_ComputerSystem"))
                {

                    query.Options.Timeout = new TimeSpan(0, 0, 30);
                    foreach (ManagementObject wmiObject in query.Get())
                    {
                        RamMemorySIze = Convert.ToDouble(wmiObject["TotalPhysicalMemory"]);
                    }
                }
            }
            catch { }

            HostName = Environment.MachineName;

            UserName = Environment.UserName;
        }
    }
}