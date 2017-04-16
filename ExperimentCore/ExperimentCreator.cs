using System.IO;

namespace ExperimentCore
{
    public class ExperimentCreator
    {
        public void CreateExperiment(string filePath, byte[] clusterData, int numOfClusters)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < numOfClusters; i++)
                {
                    file.Write(clusterData, 0, clusterData.Length);
                }
            }
        }
    }
}