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
            List<Shooter> shooterData = new List<Shooter>();
            List<ShooterAgg> finalList = new List<ShooterAgg>();

            //Get the list of matches that we want to consider when processing.
            //Each match is simply an ID from the Practiscore web site using the old html format
            //For each match get the shooter information
            string[] matches = System.IO.File.ReadAllLines("MatchList.txt");
            if (matches != null)
            {
                foreach (string matchID in matches)
                {
                    shooterData.AddRange(GetSingleMatch(matchID));
                }
            }

            //Now we have the shooter information workout who is shooting without being a member
            List<ShooterAgg> shooterSummary = new List<ShooterAgg>();

            shooterSummary = GenerateListOfMissingMemberships(shooterData);
            finalList = GetListOfCurrentMissingMemberships(shooterSummary, shooterData);
            Console.ReadLine();
        }

        private static List<ShooterAgg> GetListOfCurrentMissingMemberships(List<ShooterAgg> shooterSummary, List<Shooter> shooterData)
        {
            List<ShooterAgg> finalList = new List<ShooterAgg>();

            if (shooterSummary != null)
            {
                foreach (var badShooter in shooterSummary)
                {
                    var currentMember =
                        from m in shooterData
                        where m.fullName == badShooter.fullName
                        where m.memberId.Length >= 4
                        select m;

                    if (currentMember != null)
                    {
                        finalList.Add(badShooter);
                    }
                }
            }
            return finalList;
        }

        private static List<ShooterAgg> GenerateListOfMissingMemberships(List<Shooter> shooterData)
        {
            //Filter out shooter records shorter than 4 which is a proxy for an empty IDPA number or a "Pen" number
            //For each shooter count the number of entries they have with an empty shooter number
            var notMembers =
                    from m in shooterData
                    where (m.memberId.Length < 4)
                    group m by m.fullName into g
                    select new
                    {
                        fullName = g.Key,
                        matchCount = g.Count(),
                    };

            //Now filter out the folks with only one match with no number
            //HACK: There must be a way to do this integrated into the query above but I don't have the time to work it out
            var notMembersMoreThanOnce =
                    from s in notMembers
                    where s.matchCount > 1
                    select s;

            List<ShooterAgg> shAgg = new List<ShooterAgg>();
            if (notMembersMoreThanOnce != null)
            {
                foreach (var badShooter in notMembersMoreThanOnce)
                {
                    ShooterAgg thisShooter = new ShooterAgg();
                    thisShooter.fullName = badShooter.fullName;
                    thisShooter.matchCount = badShooter.matchCount;
                    shAgg.Add(thisShooter);
                }
            }
            return shAgg;
        }

        private static List<Shooter> GetSingleMatch(string matchID)
        {
            //Set up the correct web address for the match definition json file and get it, its not large so no need to stream
            string sourceUri = "https://s3.amazonaws.com/ps-scores/production/" + matchID + "/match_def.json";

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            string data = client.DownloadString(sourceUri);

            //Parse the JSON so we can use LINQ Query Syntax over it
            //Get all the shooter registration data from the match and distill down to name(s), number and add in the match name
            JObject shooterData = JObject.Parse(data);
            string matchTitle = shooterData["match_name"].ToString();
            var shooters =
                from s in shooterData["match_shooters"]
                select new { lastName = (string)s["sh_ln"], firstName = (string)s["sh_fn"], memberId = (string)s["sh_id"], matchName = matchTitle };

            //Weird side effect of using anonymous types, they can not be returned from methods so we have to construct a collection of typed classes
            //that can be returned. Use a List<T> because it is simple and efficient and lets us use LINQ Query syntax later.
            //Shooter class is created so it can contain the data in a strongly typed way.
            List<Shooter> typedShooters = new List<Shooter>();
            if (shooters != null)
            {
                foreach (var shtr in shooters)
                {
                    Shooter thisShooter = new Shooter();
                    thisShooter.matchName = shtr.matchName;
                    thisShooter.memberId = shtr.memberId;
                    thisShooter.firstName = shtr.firstName;
                    thisShooter.lastName = shtr.lastName;
                    thisShooter.fullName = shtr.lastName + ", " + shtr.firstName;
                    typedShooters.Add(thisShooter);
                }
            }
            return typedShooters;
        }
    }
}
