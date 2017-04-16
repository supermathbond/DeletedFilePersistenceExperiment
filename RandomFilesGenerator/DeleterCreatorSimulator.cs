using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace RandomFilesGenerator
{
    public class DeleterCreatorSimulator
    {
        private Configuration _conf;

        public DeleterCreatorSimulator(Configuration conf)
        {
            _conf = conf;
        }

        public void Run()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            Random r = new Random((int)t.TotalSeconds);

            do
            {
                bool wasChanged = false;

                if (r.Next(_conf.FileDeletionRandomOddsPerSecond) == 1)
                {
                    int index = r.Next(_conf.CreatedFiles.Count);
                    Debug.Write("RandomFileGenerator: file deleted: " + _conf.CreatedFiles[index]);
                    File.Delete(_conf.CreatedFiles[index]);
                    _conf.CreatedFiles.RemoveAt(index);
                    wasChanged = true;
                }

                if (DeleteBiggestFileIfNeeded())
                    wasChanged = true;

                int maxSize = (int)Math.Min(GetTotalFreeSpace() - _conf.MinimumFreeSpace, _conf.MaximumSizeOfFile);
                if (r.Next(_conf.FileCreationRandomOddsPerSecond) == 1)
                {
                    int size = r.Next(_conf.MinimumSizeOfFile, maxSize);
                    string folder = _conf.WorkFolders[r.Next(_conf.WorkFolders.Count)];
                    List<string> folders = Directory.GetDirectories(folder).ToList();
                    folders.Add(folder);

                    folder = folders[r.Next(folders.Count)];
                    string file = Path.Combine(folder, Guid.NewGuid().ToString());

                    byte[] buffer = new byte[50000];
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = (int)'A';
                    }

                    using (FileStream writer = new FileStream(file, FileMode.CreateNew, FileAccess.Write))
                    {
                        for(int i = 0; i < size; i += buffer.Length)
                        {
                            int amountToWrite = (buffer.Length + i <= size) ? buffer.Length : size - i;
                            writer.Write(buffer, 0, amountToWrite);
                        }
                    }
                    _conf.CreatedFiles.Add(file);
                    Debug.Write("RandomFileGenerator: file created: " + file);
                    wasChanged = true;
                }

                if(wasChanged)
                    Configuration.Serialize(_conf, AppDomain.CurrentDomain.BaseDirectory);

                Thread.Sleep(1000);
            }
            while (true);
        }

        private bool DeleteBiggestFileIfNeeded()
        {
            if (GetTotalFreeSpace() > _conf.MinimumFreeSpace)
                return false;

            List<FileInfo> files = new List<FileInfo>(_conf.CreatedFiles.Count);

            foreach (string createdFile in _conf.CreatedFiles)
            {
                files.Add(new FileInfo(createdFile));
            }

            while (GetTotalFreeSpace() <= _conf.MinimumFreeSpace)
            {
                files.OrderByDescending(x => x.Length);

                Debug.Write("not enough space on hardisk. trying to clean some...");
                if (_conf.CreatedFiles.Count > 0)
                {
                    File.Delete(files[0].FullName);
                    Debug.Write("RandomFileGenerator: file deleted: " + _conf.CreatedFiles[0]);
                    _conf.CreatedFiles.Remove(files[0].FullName);
                    files.RemoveAt(0);
                }
            }
            return true;
        }

        private long GetTotalFreeSpace()
        {
            return DriveInfo.GetDrives().Where(x => x.Name.ToLower().Contains("c:")).First().TotalFreeSpace;
        }
    }
}
