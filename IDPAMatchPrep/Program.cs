using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

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
            List<ShooterErrorRec> errRec = new List<ShooterErrorRec>();
            bool numberMatch = false;
            bool firstNameMatch = false;
            bool lastNameMatch = false;

            foreach (IDPABaseShooter aShooter in registeredShooters)
            {
                foreach (IDPAWebShooterDetails webShooter in webShooterInformation)
                {
                    if (aShooter.memberNumber.ToUpper() == webShooter.memberNumber.ToUpper()) {
                        numberMatch = true;
                    }
                    if (NameMatch(aShooter.lastName, webShooter.lastName)){
                        lastNameMatch = true;
                    }
                    if (NameMatch(aShooter.firstName, webShooter.firstName)) {
                        firstNameMatch = true;
                    }

                    if (numberMatch) 
                    {
                        if ((firstNameMatch) && (lastNameMatch)) { break; }
                        if ((!firstNameMatch) || (!lastNameMatch))
                        {
                            ShooterErrorRec rec = CreateShooterComparisonErrorRecord(aShooter, webShooter, ErrorClass.Name, "Failed to match name");
                            errRec.Add(rec);
                            break;
                        }
                    }
                }
                if (!numberMatch)
                {
                    ShooterErrorRec rec = CreateShooterComparisonErrorRecord(aShooter, null, ErrorClass.Number, "Failed to match member number");
                    errRec.Add(rec);
                }
                numberMatch = false;
                firstNameMatch = false;
                lastNameMatch = false;
            }
            if (errRec.Count > 0)
            {
                ComparisonRecordErrHandler(errRec);
            }
        }

        private static ShooterErrorRec CreateShooterComparisonErrorRecord(IDPABaseShooter aShooter, IDPAWebShooterDetails webShooter, ErrorClass eClass, string errMsg)
        {
            ShooterErrorRec rec = new ShooterErrorRec();
            rec.baseShooter = aShooter;
            rec.webShooter = webShooter;
            rec.eClass = eClass;
            rec.errorMsg = errMsg;
            return rec;
        }

        private static void ComparisonRecordErrHandler(List<ShooterErrorRec> errRec)
        {
            const int MAXSTRINGPERERROR = 150;

            StringBuilder sb = new StringBuilder();
            sb.Capacity = MAXSTRINGPERERROR;
            foreach (ShooterErrorRec err in errRec)
            {
                if (err.webShooter != null)
                {
                    sb.Append(err.baseShooter.memberNumber);
                    sb.Append(", ");
                    sb.Append(err.baseShooter.firstName);
                    sb.Append(", ");
                    sb.Append(err.baseShooter.lastName);
                    sb.Append(" : ");
                    sb.Append(err.webShooter.memberNumber);
                    sb.Append(", ");
                    sb.Append(err.webShooter.firstName);
                    sb.Append(", ");
                    sb.Append(err.webShooter.lastName);
                }
                else
                {
                    sb.Append(err.baseShooter.memberNumber);
                    sb.Append(", ");
                    sb.Append(err.baseShooter.firstName);
                    sb.Append(", ");
                    sb.Append(err.baseShooter.lastName);
                    sb.Append(" : <<NULL>>");
                }
                Console.WriteLine(sb);
                Console.WriteLine("   " + err.errorMsg);
                sb.Clear();
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
                    searchIndex = upperA.IndexOf(upperB);
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
