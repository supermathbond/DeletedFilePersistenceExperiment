using System;
using System.IO;
using System.Xml.Serialization;
using static ExperimentCore.NtfsHelper;

namespace ExperimentCore
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
            catch (Exception e)
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
            if (DetailsOfAttack == null)
            {
                DetailsOfAttack = new AttackDetails();
            }
        }

        [XmlElement(Type = typeof(int), ElementName = "Id", IsNullable = false)]
        public int Id { get; set; }
        
        [XmlElement(Type = typeof(DateTime), ElementName = "LastCheckInDb", IsNullable = false)]
        public DateTime LastCheckInDb { get; set; }

        [XmlElement(Type = typeof(bool), ElementName = "InTheMiddleOfMission", IsNullable = false)]
        public bool InTheMiddleOfMission { get; set; }

        [XmlElement(Type = typeof(AttackDetails), ElementName = "DetailsOfAttack", IsNullable = true)]
        public AttackDetails DetailsOfAttack;

        public void ResetAllConfigurationExceptId()
        {
            DetailsOfAttack = null;
        }
    }

    public class AttackDetails
    {
        public Extents FileLocationData { get; set; }

        public string FilePath { get; set; }

        [XmlIgnore]
        public byte[] DataToSearchFor { get; set; }

        [XmlElement]
        public string ProxyDataToSearchFor
        {
            get { return (DataToSearchFor == null) ? "" : Convert.ToBase64String(DataToSearchFor); }
            set { DataToSearchFor = (value == null)? new byte[0] : Convert.FromBase64String(value); }
        }

        public int MissionId { get; set; }

        public int TimeGap { get; set; }

        [XmlIgnore]
        public DateTime WhenToStart { get; set; }

        [XmlElement]
        public long ProxyDate
        {
            get { return WhenToStart.ToUnixTime(); }
            set { WhenToStart = value.FromUnixTime(); }
        }

        [XmlElement(Type = typeof(bool), ElementName = "InTheMiddleOfScan", IsNullable = false)]
        public bool InTheMiddleOfScan { get; set; }
    }
}