using System;
using System.Collections.Generic;
using ShooterUtils;
using System.Linq;

namespace VerifyClassification
{
    class Program
    {
        private static DateTime matchDate = DateTime.Now;

        static void Main(string[] args)
        {
            List<IDPABaseShooter> registeredShooters = ShooterIO.ReadRegisteredShooterList();
            List<IDPAWebShooterDetails> webShooterInformation = ShooterIO.ReadIDPAWebShooterList();

            var joinedShooters = from idpaShooter in webShooterInformation
                                 join regShooter in registeredShooters on idpaShooter.memberNumber equals regShooter.memberNumber
                                 select new IDPAWebShooterDetails {
                                     shooterDivision = regShooter.shooterDivision,
                                     shooterClass = regShooter.shooterClass,
                                     memberNumber = idpaShooter.memberNumber,
                                     firstName = regShooter.firstName,
                                     lastName = regShooter.lastName,
                                     classCDP = idpaShooter.classCDP,
                                     classESP = idpaShooter.classESP,
                                     classSSP = idpaShooter.classSSP,
                                     classCCP = idpaShooter.classCCP,
                                     classREV = idpaShooter.classREV,
                                     classBUG = idpaShooter.classBUG,
                                     lastCDP = idpaShooter.lastCDP,
                                     lastESP = idpaShooter.lastESP,
                                     lastSSP = idpaShooter.lastSSP,
                                     lastCCP = idpaShooter.lastCCP,
                                     lastREV = idpaShooter.lastREV,
                                     lastBUG = idpaShooter.lastBUG
                                 };
            ApproveEasy(joinedShooters);
            Console.ReadLine();
        }

        static void ApproveEasy(IEnumerable<IDPAWebShooterDetails> webShooterInformation)
        {
            List<IDPAWebShooterDetails> resultsList = new List<IDPAWebShooterDetails>();

            resultsList = VerifyMasters(webShooterInformation);
            resultsList.AddRange(VerifyRecentClassifier(webShooterInformation));

            Console.ReadLine();
        }

        static List<IDPAWebShooterDetails> VerifyMasters(IEnumerable<IDPAWebShooterDetails> webShooterInformation)
        {
            List<IDPAWebShooterDetails> results = new List<IDPAWebShooterDetails>();

            var mastersOnly = from idpaShooter in webShooterInformation
                              where (idpaShooter.shooterClass == "MA") || (idpaShooter.shooterClass == "DM")
                              select idpaShooter;
            foreach (IDPAWebShooterDetails shooter in mastersOnly)
            {
                switch (shooter.shooterClass)
                {
                    case "CDP":
                        {
                            if ((shooter.classCDP != "MA") && (shooter.classCDP != "DM"))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "ESP":
                        {
                            if ((shooter.classESP != "MA") && (shooter.classESP != "DM"))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "SSP":
                        {
                            if ((shooter.classSSP != "MA") && (shooter.classSSP != "DM"))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "CCP":
                        {
                            if ((shooter.classCCP != "MA") && (shooter.classCCP != "DM"))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "REV":
                        {
                            if ((shooter.classREV != "MA") && (shooter.classREV != "DM"))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "BUG":
                        {
                            if ((shooter.classBUG != "MA") && (shooter.classBUG != "DM"))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    default:
                        break;
                }
            }


            return results;
        }

        static List<IDPAWebShooterDetails> VerifyRecentClassifier(IEnumerable<IDPAWebShooterDetails> webShooterInformation)
        {
            List<IDPAWebShooterDetails> results = new List<IDPAWebShooterDetails>();

            var notMasters = from idpaShooter in webShooterInformation
                             where (idpaShooter.shooterClass != "MA") || (idpaShooter.shooterClass != "DM")
                             select idpaShooter;
            foreach (IDPAWebShooterDetails shooter in notMasters)
            {
                switch (shooter.shooterClass)
                {
                    case "CDP":
                        {
                            if (!CompareDates(matchDate, shooter.lastCDP))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "ESP":
                        {
                            if (!CompareDates(matchDate, shooter.lastESP))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "SSP":
                        {
                            if (!CompareDates(matchDate, shooter.lastSSP))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "CCP":
                        {
                            if (!CompareDates(matchDate, shooter.lastCCP))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "REV":
                        {
                            if (!CompareDates(matchDate, shooter.lastREV))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    case "BUG":
                        {
                            if (!CompareDates(matchDate, shooter.lastBUG))
                            {
                                results.Add(shooter);
                            }
                            break;
                        }
                    default:
                        break;
                }
            }


            return results;
        }



        private static bool CompareDates(DateTime matchDate, DateTime classDate)
        {
            bool result = false;

            int dateDiff = (matchDate - classDate).Days;
            if (dateDiff <= 365)
            {
                result = true;
            }
            return result;
        }

    }
}

// Rules for Division Validity
// 1. If the shooter is a MA or DM in match division good to go.
// 2. If the shooter has a classifier score in the match division in the 365 days before the date of the match they are good to go.

//Rules for Class Validity
// 1. If they are a REV or BUG shooter whatever the database says as class is correct.
// 2. If they are shooting in one of the Universal Semi-Auto Divisions then
//      a. If they have shot a classifier since Jan 1 2017 in the match division the database is correct in regard to class
//      b. If they have shot a classifier since Jan 1 2017 in ANY US-AD and that is higher than the class for the match division then that supercedes the database
//      c. If their class in the match division is 2 or more classes lower than their highest in ANY of the US-AD's then their class for the match is one lower than their highest in US-AD
// 3. If they are shooting PCC or any other SPD then their class is the same as the highest class in any other division


