using System;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            string zipPath = @"C:\dev\data\IDPAScoring\TestPSC.psc";
            string targetFileName = "match_def.json";

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName == targetFileName)
                    {
                        using (var zipStream = entry.Open()){
                            StreamReader reader = new StreamReader( zipStream );
                            string data = reader.ReadToEnd();
                            JObject shooterData = JObject.Parse(data);
                            Console.WriteLine(shooterData);
                            Console.ReadLine();
                        }
                    }
                }
            } 
        }
    }
}
