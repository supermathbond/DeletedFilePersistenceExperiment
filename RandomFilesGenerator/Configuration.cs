using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace RandomFilesGenerator
{
    [Serializable]
    public class Configuration
    {
        public const string FILE_NAME = "Conf.xml";
        public const string BACKUP_FILE_NAME = "Conf.bak.xml";

        public static Configuration DeSerialize(string baseFolder)
        {
            string mainDestFilePath = Path.Combine(baseFolder, FILE_NAME);
            string backupFilePath = Path.Combine(baseFolder, BACKUP_FILE_NAME);

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                using (Stream stream = new FileStream(mainDestFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Configuration conf = (Configuration)serializer.Deserialize(stream);
                    return conf;
                }
            }
            catch 
            {
                try
                {
                    //if(new FileInfo(mainDestFilePath).Length == 0)
                    File.Delete(mainDestFilePath);
                }
                catch (Exception) { }

                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                using (Stream stream = new FileStream(backupFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Configuration conf = (Configuration)serializer.Deserialize(stream);
                    return conf;
                }
            }
        }

        public static void Serialize(Configuration conf, string baseFolder)
        {
            string mainDestFilePath = Path.Combine(baseFolder, FILE_NAME);
            string backupFilePath = Path.Combine(baseFolder, BACKUP_FILE_NAME);

            //first, delete target file if exists, as File.Move() does not support overwrite
            if (File.Exists(mainDestFilePath))
            {
                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
                }
                File.Move(mainDestFilePath, backupFilePath);
            }

            using (Stream stream = new FileStream(mainDestFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                XmlSerializer xmlserializer = new XmlSerializer(typeof(Configuration));                
                xmlserializer.Serialize(stream, conf);
            }
        }

        public Configuration()
        {
            if (WorkFolders == null)
            {
                WorkFolders = new List<string>();
            }

            if (CreatedFiles == null)
            {
                CreatedFiles = new List<string>();
            }
        }

        [XmlElement]
        public int MinimumFreeSpace{ get; set; }

        [XmlElement]
        public int FileCreationRandomOddsPerSecond { get; set; }

        [XmlElement]
        public int FileDeletionRandomOddsPerSecond { get; set; }

        [XmlElement]
        public int MinimumSizeOfFile { get; set; }

        [XmlElement]
        public int MaximumSizeOfFile { get; set; }

        [XmlArray("WorkFolders")]
        [XmlArrayItem("WorkFolder")]
        public List<string> WorkFolders { get; set; }

        [XmlArray("CreatedFiles")]
        [XmlArrayItem("CreatedFile")]
        public List<string> CreatedFiles { get; set; }
    }
}