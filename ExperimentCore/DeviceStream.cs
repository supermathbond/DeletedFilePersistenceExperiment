using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace ExperimentCore
{
    public class DeviceStream : Stream, IDisposable
    {
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        
        public const short FILE_ATTRIBUTE_NORMAL = 0x80;
        public const short INVALID_HANDLE_VALUE = -1;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;

        // Use interop to call the CreateFile function.
        // For more information about CreateFile,
        // see the unmanaged MSDN reference library.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(
            IntPtr hFile, // handle to file
            byte[] lpBuffer, // data buffer
            int nNumberOfBytesToRead, // number of bytes to read
            ref int lpNumberOfBytesRead, // number of bytes read
            IntPtr lpOverlapped
            //
            // ref OVERLAPPED lpOverlapped        // overlapped buffer
            );

        // !!!
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SetFilePointer(IntPtr handle, int lDistanceToMove, out int lpDistanceToMoveHigh,
            uint dwMoveMethod);

        [DllImport("kernel32.dll")]
static extern uint GetLastError();

        private SafeFileHandle handleValue = null;
        private FileStream _fs = null;

        public DeviceStream(string device)
        {
            Load(device);
        }

        private void Load(string Path)
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new ArgumentNullException("Path");
            }

            // Try to open the file.
            IntPtr ptr = CreateFile(Path, GENERIC_READ,
                //0,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            handleValue = new SafeFileHandle(ptr, true);
            _fs = new FileStream(handleValue, FileAccess.Read);

            // If the handle is invalid,
            // get the last Win32 error 
            // and throw a Win32Exception.
            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            return;
        }

        public override long Length
        {
            get { return -1; }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and 
        /// (offset + count - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream. </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            var innerBuffer = new byte[count];
            if (!ReadFile(handleValue.DangerousGetHandle(), innerBuffer, count, ref bytesRead, IntPtr.Zero))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            for (int i = 0; i < bytesRead; i++)
            {
                buffer[offset + i] = innerBuffer[i];
            }
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            uint moveMethod = 0;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    moveMethod = 0;
                    break;

                case SeekOrigin.Current:
                    moveMethod = 1;
                    break;

                case SeekOrigin.End:
                    moveMethod = 2;
                    break;
            }

            int lo = (int) (offset & 0xffffffff);
            int hi = (int) (offset >> 32);

            lo = SetFilePointer(handleValue.DangerousGetHandle(), lo, out hi, moveMethod);

            if (lo == -1)
            {
                //if (GetLastError() != 0)
                //{
                    throw new Exception("INVALID_SET_FILE_POINTER");
                //}
            }

            return (((long) hi << 32) | (uint) lo);
        }

        public override int ReadByte()
        {
            int BytesRead = 0;
            var lpBuffer = new byte[1];
            if (!ReadFile(
                handleValue.DangerousGetHandle(), // handle to file
                lpBuffer, // data buffer
                1, // number of bytes to read
                ref BytesRead, // number of bytes read
                IntPtr.Zero
                ))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                ;
            }
            return lpBuffer[0];
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            handleValue.Close();
            handleValue.Dispose();
            handleValue = null;
            base.Close();
        }

        private bool disposed = false;

        private new void Dispose()
        {
            Dispose(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        private new void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (handleValue != null)
                    {
                        _fs.Dispose();
                        handleValue.Close();
                        handleValue.Dispose();
                        handleValue = null;
                    }
                }
                // Note disposing has been done.
                disposed = true;

            }
        }

    }
}