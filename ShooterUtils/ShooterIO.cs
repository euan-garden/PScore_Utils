using System;
using System.Collections.Generic;
using System.IO;

namespace ShooterUtils
{
    public static class ShooterIO
    {
        private static string regFileName = "C:\\dev\\data\\csv\\MatchMembers\\PS-Data.csv";
        private static string webFileName = "C:\\dev\\data\\csv\\MatchMembers\\HQ-Data.csv";

        public static List<IDPABaseShooter> ReadRegisteredShooterList()
        {
            List<IDPABaseShooter> shooterList = new List<IDPABaseShooter>();

            int loopCount = 1;
            foreach (string line in File.ReadAllLines(regFileName))
            {
                if (loopCount > 1)
                {
                    //TODO: Read this from a config file so that if the location of the fields changes in the data file it can be fixed in the config file
                    //not by recompiling the code
                    string[] fields = line.Split(",");
                    IDPABaseShooter aShooter = new IDPABaseShooter();
                    aShooter.firstName = fields[0];
                    aShooter.lastName = fields[1];
                    aShooter.memberNumber = fields[2];
                    aShooter.shooterClass = fields[4];
                    aShooter.shooterDivision = fields[3];
                    shooterList.Add(aShooter);
                }
                loopCount++;
            }
            return shooterList;
        }

        public static List<IDPAWebShooterDetails> ReadIDPAWebShooterList()
        {
            List<IDPAWebShooterDetails> shooterList = new List<IDPAWebShooterDetails>();

            int loopCount = 1;
            foreach (string line in File.ReadAllLines(webFileName))
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

        public static DateTime DateConversionHelper(string source)
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
