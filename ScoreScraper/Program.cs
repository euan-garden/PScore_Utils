using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ScoreScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            string matchID = "7d16e9af-8217-4494-8a10-7348995f9519";
            string sourceUri = "https://s3.amazonaws.com/ps-scores/production/" + matchID + "/match_def.json";

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            string data = client.DownloadString(sourceUri);
            JObject shooterData = JObject.Parse(data);
            var shooters =
                from s in shooterData["match_shooters"]
                select new {lastName = (string)s["sh_ln"], firstName = (string)s["sh_fn"], memberId = (string)s["sh_id"]};

            foreach(var shtr in shooters)
            {
                if (shtr.memberId.Length <= 4)
                {
                    Console.WriteLine(shtr.memberId + ", " + shtr.firstName + ", " + shtr.lastName);
                }
            }
            Console.ReadLine();
        }
    }
}



