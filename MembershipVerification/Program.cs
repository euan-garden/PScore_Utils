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
        private static string matchList = "MatchList.txt";
        private static string userAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
        private static string uriPrefix = "https://s3.amazonaws.com/ps-scores/production/";
        private static string uriSuffix = "/match_def.json";
        private static int matchStringLength = 36;

        static void Main(string[] args)
        {
            List<Shooter> shooterData = new List<Shooter>();
            List<ShooterAgg> finalList = new List<ShooterAgg>();

            //Get the list of matches that we want to consider when processing from the text file
            //Each match is simply an ID from the Practiscore web site using the old html format
            //For each match get the shooter information

            Console.WriteLine("Reading match list");
            string[] matches = System.IO.File.ReadAllLines(matchList);
            if (matches != null)
            {
                Console.WriteLine("Getting data for each match");
                int loopCount = 1;
                string shortMatchID;
                foreach (string matchID in matches)
                {
                    shortMatchID = matchID.Substring(0,matchStringLength);
                    Console.WriteLine("     Getting match " + loopCount.ToString());
                    shooterData.AddRange(GetSingleMatch(shortMatchID));
                    loopCount++;
                }
            }

            //Now we have the shooter information work out who is shooting without being a member
            List<ShooterAgg> shooterSummary = new List<ShooterAgg>();

            Console.WriteLine("Computing shooters with any match with no ID");
            shooterSummary = GenerateListOfMissingMemberships(shooterData);
            Console.WriteLine("Verify shooters have not shot a subsequant match with ID");
            finalList = GetListOfCurrentMissingMemberships(shooterSummary, shooterData);
            OutputPrint(finalList);
            Console.WriteLine("Finished");
            Console.WriteLine("Press Any Key");
            Console.ReadLine();
        }

        //Print final list
        private static void OutputPrint(List<ShooterAgg> finalList)
        {
            using (var writer = File.CreateText("OutputResults.log"))
            {
                foreach (ShooterAgg shooterEntry in finalList)
                {
                    writer.WriteLine(shooterEntry.fullName + ": " + shooterEntry.matchCount.ToString() + " Matches");
                }
            }
        }

        //Take in the list of shooters we think are breaking the rules and try and look them up to see if they have subsequantly shot a match
        //With a valid ID
        //Return the list of shooters that we still think are breaking the rules.
        private static List<ShooterAgg> GetListOfCurrentMissingMemberships(List<ShooterAgg> shooterSummary,   //Agg'd list of shooters with more than 1 match with no number
                                                                            List<Shooter> shooterData         //Raw list of shooters to look up against
                                                                           )
        {
            List<ShooterAgg> finalList = new List<ShooterAgg>();

            //For each shooter with more than 1 missing ID match verify if they have shot at least one match with an ID by essentially performing a lookup
            //Using a filter instead of a join
            if (shooterSummary != null)
            {
                foreach (var badShooter in shooterSummary)
                {
                    //Is there a record for this shooter where their number is bigger than 4
                    //Thats the proxy for having a number
                    var currentMember =
                        from m in shooterData
                        where m.fullName == badShooter.fullName
                        where m.memberId.Length >= 4
                        select m;


                    if (currentMember.Count() == 0)
                    {
                        finalList.Add(badShooter);
                    }
                }
            }
            return finalList;
        }

        //Take in the raw list of shooters one record for each match in each match, filter in only those records that have no IDPA number
        //By using 4 as a proxy for a valid number or not. For the records where they have no number group by and count by name to see
        //How many matches they have shot without a number, filter out only shooting a single match without a number
        //Return the list of shooters with more than 1 without an IDPA Number
        private static List<ShooterAgg> GenerateListOfMissingMemberships(List<Shooter> shooterData //Raw list of shooters
                                                                        )
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

            //Turn anonymous types into strong typed list so it can be returned from the method
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

        //For a given match id get the JSON data for all the shooters that shot the match and the match name 
        //Return a list of shooter records with all the info thats needed later
        private static List<Shooter> GetSingleMatch(string matchID)
        {
            //Set up the correct web address for the match definition json file and get it, its not large so no need to stream
            string sourceUri = uriPrefix + matchID + uriSuffix;

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", userAgent);

            string data = client.DownloadString(sourceUri);
            List<Shooter> typedShooters = new List<Shooter>();
            //Parse the JSON so we can use LINQ Query Syntax over it
            //Get all the shooter registration data from the match and distill down to name(s), number and add in the match name
            try
            {
                JObject shooterData = JObject.Parse(data);
                string matchTitle = shooterData["match_name"].ToString();
                var shooters =
                    from s in shooterData["match_shooters"]
                    select new { lastName = (string)s["sh_ln"], firstName = (string)s["sh_fn"], memberId = (string)s["sh_id"], matchName = matchTitle };

                //Weird side effect of using anonymous types, they can not be returned from methods so we have to construct a collection of typed classes
                //that can be returned. Use a List<T> because it is simple and efficient and lets us use LINQ Query syntax later.
                //Shooter class is created so it can contain the data in a strongly typed way.

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
            }
            catch(Exception e){
                Console.WriteLine("Problem reading the Json: " + e.Message);
                using (var writer = File.CreateText(matchID + "_ErrorResults.json"))
                {
                    writer.Write(data);
                }
            }
            return typedShooters;
        }
    }
}
