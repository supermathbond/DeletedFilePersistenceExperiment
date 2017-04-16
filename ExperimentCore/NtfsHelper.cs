﻿//
// a set of simple C# wrappers over the NT Defragmenter APIs
//
/// From: https://blogs.msdn.microsoft.com/jeffrey_wall/2004/09/13/defrag-api-c-wrappers/
//
// Refrences

//
// http://www.sysinternals.com/ntw2k/info/defrag.shtml
// 
// msdn how-to
// ms-help://MS.MSDNQTR.2004JUL.1033/fileio/base/defragmenting_files.htm
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/base/defragmenting_files.asp
//
// FSCTL_GET_VOLUME_BITMAP
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/base/fsctl_get_volume_bitmap.asp
//
// interesting structures...
// FSCTL_MOVE_FILE
// FSCTL_GET_RETRIEVAL_POINTERS
// RETRIEVAL_POINTERS_BUFFER
// FSCTL_GET_RETRIEVAL_POINTERS
//
// DeviceIoControl
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/devio/base/deviceiocontrol.asp
//

using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ExperimentCore
{
    public class NtfsHelper
    {
        public const int CLUSTER_SIZE = 1024 * 4;

        //
        // CreateFile constants
        //
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        const uint FILE_SHARE_DELETE = 0x00000004;
        const uint OPEN_EXISTING = 3;

        const uint GENERIC_READ = (0x80000000);
        const uint GENERIC_WRITE = (0x40000000);

        const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
        const uint FILE_READ_ATTRIBUTES = (0x0080);
        const uint FILE_WRITE_ATTRIBUTES = 0x0100;
        const uint ERROR_INSUFFICIENT_BUFFER = 122;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            [Out] IntPtr lpOutBuffer,
            uint nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped);

        static private IntPtr OpenVolume(string deviceName)
        {
            IntPtr hDevice = CreateFile(
                @"\\.\" + deviceName,
                GENERIC_READ,// | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,//FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if ((int)hDevice == -1)
            {
                throw new Exception(Marshal.GetLastWin32Error().ToString());
            }
            return hDevice;
        }

        static private IntPtr OpenFile(string path)
        {
            IntPtr hFile = CreateFile(
                path,
                FILE_READ_ATTRIBUTES | FILE_WRITE_ATTRIBUTES,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);
            if ((int)hFile == -1)
            {
                throw new Exception(Marshal.GetLastWin32Error().ToString());
            }
            return hFile;
        }

        [Serializable]
        public struct Extents
        {
            public ulong ExtentCount;
            public long StartingVcn;
            public long[] NextVcn;
            public long[] Lcn;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Temp
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] arr;
        }


        [DllImport("kernel32.dll")]
        public static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"> For example: "C:" </param>
        /// <returns></returns>
        public static BitArray GetNtfsClusterBitmap(string volumeLetter)
        {
            StringBuilder builder = new StringBuilder(260);
            QueryDosDevice(volumeLetter, builder, 260);
            return GetVolumeMap(builder.ToString().Replace(@"\Device\", ""));
        }

        /// <summary>
        /// Get cluster usage for a device
        /// </summary>
        /// <param name="deviceName">use "c:"</param>
        /// <returns>a bitarray for each cluster</returns>
        static public BitArray GetVolumeMap(string deviceName)
        {
            IntPtr pAlloc = IntPtr.Zero;
            IntPtr hDevice = IntPtr.Zero;

            try
            {
                hDevice = OpenVolume(deviceName);

                Int64 i64 = 0;

                GCHandle handle = GCHandle.Alloc(i64, GCHandleType.Pinned);
                IntPtr p = handle.AddrOfPinnedObject();

                // alloc off more than enough for my machine
                // 64 megs == 67108864 bytes == 536870912 bits == cluster count
                // NTFS 4k clusters == 2147483648 k of storage == 2097152 megs == 2048 gig disk storage
                uint q = 1024 * 1024 * 64; // 1024 bytes == 1k * 1024 == 1 meg * 64 == 64 megs

                uint size = 0;
                pAlloc = Marshal.AllocHGlobal((int)q);
                IntPtr pDest = pAlloc;

                bool fResult = DeviceIoControl(
                    hDevice,
                    FSConstants.FSCTL_GET_VOLUME_BITMAP,
                    p,
                    (uint)Marshal.SizeOf(i64),
                    pDest,
                    q,
                    ref size,
                    IntPtr.Zero);

                if (!fResult)
                {
                    throw new Exception(Marshal.GetLastWin32Error().ToString());
                }
                handle.Free();

                /*
                object returned was...
          typedef struct 
          {
           LARGE_INTEGER StartingLcn;
           LARGE_INTEGER BitmapSize;
           BYTE Buffer[1];
          } VOLUME_BITMAP_BUFFER, *PVOLUME_BITMAP_BUFFER;
                */
                Int64 startingLcn = (Int64)Marshal.PtrToStructure(pDest, typeof(Int64));

                Debug.Assert(startingLcn == 0);

                pDest = (IntPtr)((Int64)pDest + 8);
                Int64 bitmapSize = (Int64)Marshal.PtrToStructure(pDest, typeof(Int64));

                Int32 byteSize = (int)(bitmapSize / 8);
                byteSize++; // round up - even with no remainder

                IntPtr bitmapBegin = (IntPtr)((Int64)pDest + 8);

                byte[] byteArr = new byte[byteSize];

                Marshal.Copy(bitmapBegin, byteArr, 0, (Int32)byteSize);

                BitArray retVal = new BitArray(byteArr) {Length = (int) bitmapSize};
                // truncate to exact cluster count
                return retVal;
            }
            finally
            {
                CloseHandle(hDevice);
                hDevice = IntPtr.Zero;

                Marshal.FreeHGlobal(pAlloc);
                pAlloc = IntPtr.Zero;
            }
        }


        /// <summary>
        /// returns a 2*number of extents array - 
        /// the vcn and the lcn as pairs
        /// </summary>
        /// <param name="path">file to get the map for ex: "c:\windows\explorer.exe" </param>
        /// <returns>An array of [virtual cluster, physical cluster]</returns>
        static public Extents GetFileMap(string path)
        {
            IntPtr hFile = IntPtr.Zero;
            IntPtr pAlloc = IntPtr.Zero;

            try
            {
                hFile = OpenFile(path);

                long i64 = 0;

                GCHandle handle = GCHandle.Alloc(i64, GCHandleType.Pinned);
                IntPtr p = handle.AddrOfPinnedObject();

                uint size = 0;
                uint q = 1024 * 1024 * 64; // 1024 bytes == 1k * 1024 == 1 meg * 64 == 64 megs
                pAlloc = Marshal.AllocHGlobal((int)q);
                IntPtr pDest = pAlloc;
                bool fResult = DeviceIoControl(hFile, FSConstants.FSCTL_GET_RETRIEVAL_POINTERS,
                    p, (uint)Marshal.SizeOf(i64),
                    pDest, q,
                    ref size,
                    IntPtr.Zero);

                if (!fResult)
                {
                    throw new Exception(Marshal.GetLastWin32Error().ToString());
                }

                handle.Free();

                if (size % sizeof(long) != 0)
                    throw new Exception("recived bad output from DeviceIoControl. Size of: " + size);

                Extents retVal = new Extents();
                Temp temp = (Temp)Marshal.PtrToStructure(pDest, typeof(Temp));
                retVal.ExtentCount = (ulong) ConvertByteArrayToLong(temp.arr);
                //retVal.ExtentCount = (ulong)Marshal.ReadInt32(pDest);

                pDest = (IntPtr)((long)pDest + 8);

                temp = (Temp)Marshal.PtrToStructure(pDest, typeof(Temp));
                retVal.StartingVcn = ConvertByteArrayToLong(temp.arr); ;
                //retVal.StartingVcn = Marshal.ReadInt64(pDest);

                pDest = (IntPtr)((long)pDest + 8);

                // now pDest points at an array of pairs of Int64s.
                retVal.Lcn = new long[retVal.ExtentCount];
                retVal.NextVcn = new long[retVal.ExtentCount];

                for (ulong i = 0; i < retVal.ExtentCount; i++)
                {
                    temp = (Temp)Marshal.PtrToStructure(pDest, typeof(Temp));
                    retVal.NextVcn[i] = ConvertByteArrayToLong(temp.arr); ;
                    //retVal.NextVcn[i] = Marshal.ReadInt64(pDest);
                    pDest = (IntPtr)((long)pDest + 8);

                    temp = (Temp)Marshal.PtrToStructure(pDest, typeof(Temp));
                    retVal.Lcn[i] = ConvertByteArrayToLong(temp.arr); ;
                    //retVal.Lcn[i] = Marshal.ReadInt64(pDest);
                    pDest = (IntPtr)((long)pDest + 8);
                }

                return retVal;
            }
            finally
            {
                CloseHandle(hFile);
                hFile = IntPtr.Zero;

                Marshal.FreeHGlobal(pAlloc);
                pAlloc = IntPtr.Zero;
            }
        }

        private static long ConvertByteArrayToLong(byte[] arr)
        {
            long res = 0;

            for (int i = arr.Length - 1;  i >= 0;  i--)
            {
                res <<= 8;
                res += arr[i];
            }

            return res;
        }

        /// <summary>
        /// returns a 2*number of extents array – 
        /// the vcn and the lcn as pairs
        /// </summary>
        /// <param name=”path”>file to get the map for ex: “c:\windows\explorer.exe” </param>
        /// <returns>An array of [virtual cluster, physical cluster]</returns>
        static public Array OldGetFileMap(string path)
        {
            IntPtr hFile = IntPtr.Zero;
            IntPtr pAlloc = IntPtr.Zero;

            try
            {
                hFile = OpenFile(path);

                Int64 i64 = 0;

                GCHandle handle = GCHandle.Alloc(i64, GCHandleType.Pinned);
                IntPtr p = handle.AddrOfPinnedObject();

                uint q = 1024 * 1024 * 64; // 1024 bytes == 1k * 1024 == 1 meg * 64 == 64 megs

                uint size = 0;
                pAlloc = Marshal.AllocHGlobal((int)q);
                IntPtr pDest = pAlloc;
                bool fResult = DeviceIoControl(
                    hFile,
                    FSConstants.FSCTL_GET_RETRIEVAL_POINTERS,
                    p,
                    (uint)Marshal.SizeOf(i64),
                    pDest,
                    q,
                    ref size,
                    IntPtr.Zero);

                if (!fResult)
                {
                    throw new Exception(Marshal.GetLastWin32Error().ToString());
                }

                handle.Free();

                /*
                returned back one of…
     typedef struct RETRIEVAL_POINTERS_BUFFER {  
     DWORD ExtentCount;  
     LARGE_INTEGER StartingVcn;  
     struct {
         LARGE_INTEGER NextVcn;
      LARGE_INTEGER Lcn;
        } Extents[1];
     } RETRIEVAL_POINTERS_BUFFER, *PRETRIEVAL_POINTERS_BUFFER;
    */

                Int32 ExtentCount = (Int32)Marshal.PtrToStructure(pDest, typeof(Int32));

                pDest = (IntPtr)((Int64)pDest + 4);

                Int64 StartingVcn = (Int64)Marshal.PtrToStructure(pDest, typeof(Int64));

                Debug.Assert(StartingVcn == 0);

                pDest = (IntPtr)((Int64)pDest + 8);

                // now pDest points at an array of pairs of Int64s.

                Array retVal = Array.CreateInstance(typeof(Int64), new int[2] { ExtentCount, 2 });

                for (int i = 0; i < ExtentCount; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Int64 v = (Int64)Marshal.PtrToStructure(pDest, typeof(Int64));
                        retVal.SetValue(v, new int[2] { i, j });
                        pDest = (IntPtr)((Int64)pDest + 8);
                    }
                }

                return retVal;
            }
            finally
            {
                CloseHandle(hFile);
                hFile = IntPtr.Zero;

                Marshal.FreeHGlobal(pAlloc);
                pAlloc = IntPtr.Zero;
            }
        }

        /// <summary>
        /// input structure for use in MoveFile
        /// </summary>
        private struct MoveFileData
        {
            public IntPtr hFile;
            public Int64 StartingVCN;
            public Int64 StartingLCN;
            public Int32 ClusterCount;
        }

        /// <summary>
        /// move a virtual cluster for a file to a logical cluster on disk, repeat for count clusters
        /// </summary>
        /// <param name="deviceName">device to move on"c:"</param>
        /// <param name="path">file to muck with "c:\windows\explorer.exe"</param>
        /// <param name="VCN">cluster number in file to move</param>
        /// <param name="LCN">cluster on disk to move to</param>
        /// <param name="count">for how many clusters</param>
        static public void MoveFile(string deviceName, string path, Int64 VCN, Int64 LCN, Int32 count)
        {
            IntPtr hVol = IntPtr.Zero;
            IntPtr hFile = IntPtr.Zero;
            try
            {
                hVol = OpenVolume(deviceName);

                hFile = OpenFile(path);


                MoveFileData mfd = new MoveFileData
                {
                    hFile = hFile,
                    StartingVCN = VCN,
                    StartingLCN = LCN,
                    ClusterCount = count
                };

                GCHandle handle = GCHandle.Alloc(mfd, GCHandleType.Pinned);
                IntPtr p = handle.AddrOfPinnedObject();
                uint bufSize = (uint)Marshal.SizeOf(mfd);
                uint size = 0;

                bool fResult = DeviceIoControl(
                    hVol,
                    FSConstants.FSCTL_MOVE_FILE,
                    p,
                    bufSize,
                    IntPtr.Zero, // no output data from this FSCTL
                    0,
                    ref size,
                    IntPtr.Zero);

                handle.Free();

                if (!fResult)
                {
                    throw new Exception(Marshal.GetLastWin32Error().ToString());
                }
            }
            finally
            {
                CloseHandle(hVol);
                CloseHandle(hFile);
            }
        }
    }


    /// <summary>
    /// constants lifted from winioctl.h from platform sdk
    /// </summary>
    internal class FSConstants
    {
        const uint FILE_DEVICE_FILE_SYSTEM = 0x00000009;

        const uint METHOD_NEITHER = 3;
        const uint METHOD_BUFFERED = 0;

        const uint FILE_ANY_ACCESS = 0;
        const uint FILE_SPECIAL_ACCESS = FILE_ANY_ACCESS;

        public static uint FSCTL_GET_VOLUME_BITMAP = CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 27, METHOD_NEITHER, FILE_ANY_ACCESS);
        public static uint FSCTL_GET_RETRIEVAL_POINTERS = CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 28, METHOD_NEITHER, FILE_ANY_ACCESS);
        public static uint FSCTL_MOVE_FILE = CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 29, METHOD_BUFFERED, FILE_SPECIAL_ACCESS);

        static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
        {
            return ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method);
        }
    }

}