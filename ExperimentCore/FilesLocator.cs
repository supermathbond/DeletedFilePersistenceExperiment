using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace TezaExperiment
{
    public class FilesLocator
    {
        /// <summary>
        /// When a new section of clusters is being scanned, meaning no previous data is needed, 
        /// and the state can be saved, this event raises with the list of the imaged which was found until now,
        /// and the last scanned cluster index.
        /// </summary>
        /// <remarks> This event will be raise only during the FindAllMaliciousFiles method. </remarks>
        public event Action<List<MaliciousFileContainer>, int> NewClusterSectionIsBeingScanned;

        private readonly ILog _log;

        public List<MaliciousFileContainer> MaliciousImageList { get; set; }

        public SerializableDictionary<string, LegitFileContainer> LegitImageList { get; set; }

        private readonly byte[] _startOfFile;

        [DllImport("kernel32.dll")]
        static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        public FilesLocator(List<MaliciousFileContainer> files, SerializableDictionary<string, LegitFileContainer> legitImageList, ILog log)
            : this(files, legitImageList, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, log)
        { }

        public FilesLocator(List<MaliciousFileContainer> files, SerializableDictionary<string, LegitFileContainer> legitImageList, byte[] startOfFile, ILog log)
        {
            _log = log;
            MaliciousImageList = files;
            LegitImageList = legitImageList;
            for (int i = 0; i < files.Count; i++)
            {
                MaliciousImageList[i].File = File.ReadAllBytes(MaliciousImageList[i].PathOfDummyFile);
            }
            foreach (KeyValuePair<string, LegitFileContainer> legitFileContainer in LegitImageList)
            {
                legitFileContainer.Value.File = File.ReadAllBytes(legitFileContainer.Value.PathOfDummyFile);
            }

            _startOfFile = startOfFile;
        }

        private List<int> GetRange(int count)
        {
            var list = new List<int>(count);
            for (int j = 0; j < count; j++)
            {
                list.Add(j);
            }

            return list;
        }

        public List<MaliciousFileContainer> FindAllMaliciousFiles(int clusterToStartWith = 0)
        {
            using (HdWalk hdWalk = new HdWalk(_log, clusterToStartWith))
            {
                var tmpMaliciousList = MaliciousImageList;
                MaliciousImageList = new List<MaliciousFileContainer>();
                List<int> tmpList = GetRange(tmpMaliciousList.Count);
                
                do
                {
                    HdWalkResult res = hdWalk.GetNextBytes();

                    if (res == null || res.Bytes == null)
                        return MaliciousImageList;

                    if (res.IsNewSection)
                    {
                        ResetAllStates(tmpMaliciousList);
                        OnNewCLusterWasScanned(MaliciousImageList, res.LastCluster);
                    }
                    _log.Info("Reading the " + res.SeekCount + "Bytes offset of HD.");
                    int i = 0;

                    while (i < res.Bytes.Length)
                    {
                        bool foundSomeMatch = false;
                        if (i % HdWalk.CLUSTER_SIZE == 0)
                        {
                            // Reset List.
                            tmpList = GetRange(tmpMaliciousList.Count);
                        }
                        for (int j = tmpList.Count - 1; j >= 0; j--)
                        {
                            if (tmpMaliciousList[tmpList[j]].LocationOffset < _startOfFile.Length)
                            {
                                if (_startOfFile[tmpMaliciousList[tmpList[j]].LocationOffset] == res.Bytes[i])
                                {
                                    tmpMaliciousList[tmpList[j]].LocationOffset++;
                                    foundSomeMatch = true;
                                }
                                else
                                {
                                    tmpMaliciousList[tmpList[j]].LocationOffset = 0;
                                    tmpList.RemoveAt(j);
                                    continue;
                                }
                            }
                            else
                            {
                                if (tmpMaliciousList[tmpList[j]].File[tmpMaliciousList[tmpList[j]].LocationOffset] ==
                                    res.Bytes[i])
                                {
                                    tmpMaliciousList[tmpList[j]].LocationOffset++;
                                    foundSomeMatch = true;
                                }
                                else
                                {
                                    tmpMaliciousList[tmpList[j]].LocationOffset = 0;
                                    tmpList.RemoveAt(j);
                                    continue;
                                }
                            }


                            if (tmpMaliciousList[tmpList[j]].LocationOffset == tmpMaliciousList[tmpList[j]].File.Length)
                            {
                                _log.Info("Image found!! " + tmpMaliciousList[tmpList[j]].PathOfDummyFile);
                                tmpMaliciousList[tmpList[j]].LocationOffset = res.SeekCount + i + 1 -
                                                                     tmpMaliciousList[tmpList[j]].File.Length;
                                MaliciousImageList.Add(tmpMaliciousList[tmpList[j]]);
                                tmpMaliciousList.RemoveAt(tmpList[j]);
                                // Make the rest zero.
                                ResetAllStates(tmpMaliciousList);
                                // Set the flag for reset.
                                foundSomeMatch = false;
                                break;
                            }
                        }
                        if (!foundSomeMatch)
                        {
                            // Fetch next cluster.
                            i = i / HdWalk.CLUSTER_SIZE;
                            int numClusters = i;
                            i = (i + 1) * HdWalk.CLUSTER_SIZE;
                            ResetAllStates(tmpMaliciousList);

                            // Indicate that scan state may be saved.
                            OnNewCLusterWasScanned(MaliciousImageList, res.LastCluster + numClusters);
                        }
                        else
                        {
                            i++;
                        }
                    }
                } while (tmpMaliciousList.Count > 0 && !hdWalk.EndOfStream);

                return MaliciousImageList;
            }
        }

        private void ResetAllStates(List<MaliciousFileContainer> tmpMaliciousList)
        {
            for (int k = 0; k < tmpMaliciousList.Count; k++)
            {
                tmpMaliciousList[k].LocationOffset = 0;
            }
        }

        public int GetNumberOfExistingMaliciousFiles()
        {
            _log.Info("### Searching for the known malicious files...");

            int counter = 0;
            StringBuilder builder = new StringBuilder(260);
            QueryDosDevice("C:", builder, 260);
            using (BinaryReader reader = new BinaryReader(new DeviceStream(
                String.Format(@"\\.\{0}", builder.ToString().Replace(@"\Device\", "")))))//@"\\.\HarddiskVolume4")))
            {
                for (int i = MaliciousImageList.Count - 1; i >= 0; i--)
                {
                    // MUST READ IN CLUSTER SIZE CHUNKS!
                    var buffer = new byte[MaliciousImageList[i].File.Length + HdWalk.CLUSTER_SIZE - (MaliciousImageList[i].File.Length % HdWalk.CLUSTER_SIZE)];

                    try
                    {
                        reader.BaseStream.Seek(MaliciousImageList[i].LocationOffset, SeekOrigin.Begin);
                        int count = reader.Read(buffer, 0, buffer.Length);
                        if (count < MaliciousImageList[i].File.Length)
                            continue;
                    }
                    catch (Exception e)
                    {
                        _log.Error("Error At watching for files. Error: " + e.Message);
                        continue;
                    }

                    for (int j = 0; j < MaliciousImageList[i].File.Length; j++)
                    {
                        if (j < _startOfFile.Length)
                        {
                            if (buffer[j] != _startOfFile[j])
                            {
                                MaliciousImageList.RemoveAt(i);
                                break;
                            }
                        }
                        else
                        {
                            if (MaliciousImageList[i].File[j] != buffer[j])
                            {
                                MaliciousImageList.RemoveAt(i);
                                break;
                            }
                            else if (j == MaliciousImageList[i].File.Length - 1)
                            {
                                counter++;
                                break;
                            }
                        }
                    }
                }
                return counter;
            }
        }

        public int GetNumberOfExistingLegitFiles()
        {
            _log.Info("### Searching for the remaining of legit files...");

            int counter = 0;
            StringBuilder builder = new StringBuilder(260);
            QueryDosDevice("C:", builder, 260);
            using (BinaryReader reader = new BinaryReader(new DeviceStream(
                String.Format(@"\\.\{0}", builder.ToString().Replace(@"\Device\", "")))))//@"\\.\HarddiskVolume4")))
            {
                BitArray bitmap =  HdWalk.GetNtfsClusterBitmap("C:");

                for (int i = 0; i < MaliciousImageList.Count; i++)
                {
                    LegitFileContainer legitFileContainer = LegitImageList[MaliciousImageList[i].PathOfDummyFile];

                    // MUST READ IN CLUSTER SIZE CHUNKS!
                    var buffer = new byte[legitFileContainer.SizeOnDisk];

                    long locationOfLegit = MaliciousImageList[i].LocationOffset - legitFileContainer.SizeOnDisk;
                    if (bitmap[((int) (locationOfLegit / HdWalk.CLUSTER_SIZE))])
                    {
                        _log.Debug("Legit File is not set as free in bitmap YYAY!! :)");
                        //continue;
                    }

                    try
                    {
                        reader.BaseStream.Seek(locationOfLegit, SeekOrigin.Begin);
                        int count = reader.Read(buffer, 0, buffer.Length);
                        if (count < legitFileContainer.Size)
                            continue;
                    }
                    catch (Exception e)
                    {
                        _log.Error("Error At watching for files. Error: " + e);
                        continue;
                    }

                    for (int j = 0; j < legitFileContainer.File.Length; j++)
                    {
                        if (j < _startOfFile.Length)
                        {
                            if (buffer[j] != _startOfFile[j])
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (legitFileContainer.File[j] != buffer[j])
                            {
                                break;
                            }
                            else if (j == legitFileContainer.File.Length - 1)
                            {
                                if (bitmap[((int)(locationOfLegit / HdWalk.CLUSTER_SIZE))])
                                {
                                    _log.Debug("Legit File is not set as free in bitmap AND the file is the same!! YYAY!! :)");
                                    break;
                                }
                                counter++;
                                break;
                            }
                        }
                    }
                }
                return counter;
            }
        }

        protected virtual void OnNewCLusterWasScanned(List<MaliciousFileContainer> arg1, int arg2)
        {
            var handler = NewClusterSectionIsBeingScanned;
            if (handler != null) 
                handler(arg1, arg2);
        }
    }
}