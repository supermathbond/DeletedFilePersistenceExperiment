using System;
using System.Collections;
using System.IO;
using System.Text;
using log4net;
using static ExperimentCore.NtfsHelper;

namespace ExperimentCore
{
    public class LeftoverChunks
    {
        public int Lcn { get; set; }

        public float PercentageCompleted { get; set; }

        public bool IsAllocated { get; set; }
    }

    public class FilesLocatorNG
    {
        private readonly ILog _log;

        private readonly byte[] _patternToSearch;

        private Extents _fileChunks;

        public FilesLocatorNG(Extents fileChunks, byte[] patternToSearch, ILog log)
        {
            _log = log;
            _fileChunks = fileChunks;
            _patternToSearch = patternToSearch;
        }

        public long TotalNumberOfClusters(Extents extents)
        {
            long res = 0;
            long lastVcn = extents.StartingVcn;
            for (ulong i = 0; i < extents.ExtentCount; i++)
            {
                res += extents.NextVcn[i] - lastVcn;
                lastVcn = extents.NextVcn[i];
            }

            return res;
        }

        public LeftoverChunks[] GetNumberOfExistingMaliciousParts()
        {
            _log.Info("### Searching for the known malicious files...");
            
            // MUST READ IN CLUSTER SIZE CHUNKS!
            const int MAX_CLUSTERS_TO_READ = 50;
            var buffer = new byte[MAX_CLUSTERS_TO_READ * CLUSTER_SIZE];
            LeftoverChunks[] result = new LeftoverChunks[TotalNumberOfClusters(_fileChunks)];
            int leftoverChuncksIndex = 0;

            StringBuilder builder = new StringBuilder(260);
            QueryDosDevice("C:", builder, 260);
            // RUN AS ADMIN!!
            using (BinaryReader reader = new BinaryReader(new DeviceStream(
                string.Format(@"\\.\{0}", builder.ToString().Replace(@"\Device\", "")))))//@"\\.\HarddiskVolume4")))
            {
                BitArray bitmap = GetNtfsClusterBitmap("C:");

                long lastVcn = _fileChunks.StartingVcn;
                for (ulong i = 0; i < _fileChunks.ExtentCount; i++)
                {
                    long clustersInCurrentExtent = _fileChunks.NextVcn[i] - lastVcn;

                    for (int count = 0; count < clustersInCurrentExtent; count += MAX_CLUSTERS_TO_READ)
                    {
                        try
                        {
                            reader.BaseStream.Seek((_fileChunks.Lcn[i] + count) * CLUSTER_SIZE, SeekOrigin.Begin);

                            int readSize = (int)(clustersInCurrentExtent - count >= MAX_CLUSTERS_TO_READ ?
                                                        buffer.Length :
                                                        (clustersInCurrentExtent - count) * CLUSTER_SIZE);
                            
                            // ENTER BYTES NOT CLUSTER INDEX
                            int size = reader.Read(buffer, 0, readSize);
                            for (int j = 0; j < size; j += _patternToSearch.Length)
                            {
                                result[leftoverChuncksIndex] = new LeftoverChunks();
                                result[leftoverChuncksIndex].Lcn = ((int)_fileChunks.Lcn[i]) + count + (j / CLUSTER_SIZE);
                                result[leftoverChuncksIndex].IsAllocated = bitmap[result[leftoverChuncksIndex].Lcn];
                                result[leftoverChuncksIndex].PercentageCompleted = GetPercentOfOverlap(buffer, j, _patternToSearch);

                                leftoverChuncksIndex++;
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error("Error At watching for files. Error: " + e.ToString());
                            throw;
                        }
                    }

                    lastVcn = _fileChunks.NextVcn[i];

                    //_log.Debug(string.Format("Finished extent: {0}. {1}% of clusters ended.", i, leftoverChuncksIndex.PercentOf(result.Length)));
                }
            }

            return result;
        }
        
            
        private float GetPercentOfOverlap(byte[] bigBuffer, int startIndex, byte[] patternToSearch)
        {
            int counter = 0;
            for(int i = patternToSearch.Length - 1; i >= 0; i--)
            {
                if (patternToSearch[i] == bigBuffer[startIndex + i])
                    counter++;
                else
                    break;
            }

            return counter.PercentOf(patternToSearch.Length);
        }
    }
}