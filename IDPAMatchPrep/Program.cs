using System;
using System.IO;
using System.Collections.Generic;

namespace IDPAMatchPrep
{
    class Program
    {
        private static string regFileName = "Western-Entry-List.csv";
        private static string webFileName = "IDPA-Data-Western-Entries.csv";
        static void Main(string[] args)
        {
            List<IDPABaseShooter> registeredShooters = ReadRegistreredShooterList(regFileName);
            List<IDPAWebShooterDetails> webShooterInformation = ReadIDPAWebShooterList(webFileName);
            CompareTwoShooterListsForMatch(registeredShooters, webShooterInformation);
            Console.ReadLine();
        }

        private static void CompareTwoShooterListsForMatch(List<IDPABaseShooter> registeredShooters, List<IDPAWebShooterDetails> webShooterInformation)
        {
            //Low Tech for now but functional
            foreach (IDPABaseShooter aShooter in registeredShooters)
            {
                bool foundNumberMatch = false;
                bool foundFirstNameMatch = false;
                bool foundLastNameMatch = false;
                foreach (IDPAWebShooterDetails webShooter in webShooterInformation)
                {
                    if (aShooter.memberNumber.ToUpper() == webShooter.memberNumber.ToUpper())
                    {
                        foundNumberMatch = true; 
                        if (NameMatch(aShooter.lastName, webShooter.lastName))
                        {
                            foundLastNameMatch = true;
                            if (NameMatch(aShooter.firstName, webShooter.firstName))
                            {
                                foundFirstNameMatch = true;
                                break;
                            }
                        }
                    }
                }

                //TODO: Need to do a better job here in terms of reporting failures so they can be eyeballed
                if (!foundNumberMatch)
                {
                    Console.WriteLine("No number match: " + aShooter.memberNumber.ToString() + ", " + aShooter.lastName + ", " + aShooter.firstName);
                }
                if (!foundFirstNameMatch)
                {
                    Console.WriteLine("Number but no first name match: " + aShooter.memberNumber.ToString() + ", " + aShooter.lastName + ", " + aShooter.firstName);
                }
                if (!foundLastNameMatch)
                {
                    Console.WriteLine("Number but no last name match: " + aShooter.memberNumber.ToString() + ", " + aShooter.lastName + ", " + aShooter.firstName);
                }

                foundNumberMatch = false;
                foundFirstNameMatch = false;
                foundLastNameMatch = false;
            }
        }

        private static bool NameMatch(string A, string B)
        {
            bool result = false;
            string upperA = A.ToUpper();
            string upperB = B.ToUpper();
            if (upperA == upperB)
            {
                result = true;
            }
            else
            {
                int searchIndex = upperB.IndexOf(upperA);
                if (searchIndex >= 0)
                {
                    result = true;
                }
                else
                {
                    searchIndex = upperB.IndexOf(upperA);
                    if (searchIndex >= 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private static List<IDPABaseShooter> ReadRegistreredShooterList(string fileName)
        {
            List<IDPABaseShooter> shooterList = new List<IDPABaseShooter>();

            int loopCount = 1;
            foreach (string line in File.ReadAllLines(fileName))
            {
                if (loopCount > 1)
                {
                    string[] fields = line.Split(",");
                    IDPABaseShooter aShooter = new IDPABaseShooter();
                    aShooter.firstName = fields[0];
                    aShooter.lastName = fields[1];
                    aShooter.memberNumber = fields[2];
                    aShooter.shooterClass = fields[3];
                    aShooter.shooterDivision = fields[4];
                    shooterList.Add(aShooter);
                }
                loopCount++;
            }
            return shooterList;
        }

        private static List<IDPAWebShooterDetails> ReadIDPAWebShooterList(string fileName)
        {
            List<IDPAWebShooterDetails> shooterList = new List<IDPAWebShooterDetails>();

            int loopCount = 1;
            foreach (string line in File.ReadAllLines(fileName))
            {
                if (loopCount > 1)
                {
                    string[] fields = line.Split(",");
                    IDPAWebShooterDetails aShooter = new IDPAWebShooterDetails();
                    aShooter.firstName = fields[2];
                    aShooter.lastName = fields[3];
                    aShooter.memberNumber = fields[1];
                    aShooter.status = fields[4];
                    aShooter.ExpDate = DateConversionHelper(fields[5]);
                    aShooter.state = fields[6];
                    aShooter.classCDP = fields[7];
                    aShooter.classESP = fields[8];
                    aShooter.classSSP = fields[9];
                    aShooter.classCCP = fields[10];
                    aShooter.classREV = fields[11];
                    aShooter.classBUG = fields[12];
                    aShooter.lastCDP = DateConversionHelper(fields[13]);
                    aShooter.lastESP = DateConversionHelper(fields[14]);
                    aShooter.lastSSP = DateConversionHelper(fields[15]);
                    aShooter.lastCCP = DateConversionHelper(fields[16]);
                    aShooter.lastREV = DateConversionHelper(fields[17]);
                    aShooter.lastBUG = DateConversionHelper(fields[18]);

                    shooterList.Add(aShooter);
                }
                loopCount++;
            }
            return shooterList;
        }

        private static DateTime DateConversionHelper(string source)
        {
            DateTime result = DateTime.MinValue;

            if (source.Trim().Length != 0)
            {
                result = Convert.ToDateTime(source);
            }

            return result;
        }
    }

    
}
