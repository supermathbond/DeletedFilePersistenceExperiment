using System;
using System.IO;
using ExperimentCore;

namespace FileLocationTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = args[0];//@"C:\Users\anonymous\Downloads\boost_1_55_0.zip";//
            FileInfo fileInfo = new FileInfo(file);
            var x = NtfsHelper.GetFileMap(file);
            Console.WriteLine("starting VCN: " + x.StartingVcn);
            Console.WriteLine("VCN, LCN");
            for (ulong i = 0; i < x.ExtentCount; i++)
            {
                Console.WriteLine(x.NextVcn[i] + "," + x.Lcn[i]);

            }

            Console.ReadKey();

            //// MUST READ IN CLUSTER SIZE CHUNKS!
            //var buffer = new byte[fileInfo.Length + HdWalk.CLUSTER_SIZE - (fileInfo.Length % HdWalk.CLUSTER_SIZE)];

            //StringBuilder builder = new StringBuilder(260);
            //FilesLocatorNG.QueryDosDevice("C:", builder, 260);
            //// RUN AS ADMIN!!
            //using (BinaryReader reader = new BinaryReader(new DeviceStream(
            //    String.Format(@"\\.\{0}", builder.ToString().Replace(@"\Device\", "")))))//@"\\.\HarddiskVolume4")))
            //{
            //    int count = 0;
            //    long lastVcn = x.StartingVcn;
            //    for (ulong i = 0; i < x.ExtentCount; i++)
            //    {
            //        try
            //        {
            //            // ENTER BYTES NOT CLUSTER INDEX
            //            reader.BaseStream.Seek(x.Lcn[i] * HdWalk.CLUSTER_SIZE, SeekOrigin.Begin);

            //            long length = (x.NextVcn[i] - lastVcn) * HdWalk.CLUSTER_SIZE;
            //            lastVcn = x.NextVcn[i];

            //            count += reader.Read(buffer, count, (int)length);
            //        }
            //        catch (Exception e)
            //        {
            //            continue;
            //        }
            //    }
            //}

            //var buffer2 = File.ReadAllBytes(file);

            //for(int i = 0; i < fileInfo.Length; i++)
            //{
            //    if(buffer[i] != buffer2[i])
            //    {
            //        Console.WriteLine("WHY???");
            //    }
            //}

            //Console.WriteLine("Finished");
        }
    }
}
