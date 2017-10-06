using System;
using System.Collections.Generic;
using System.Text;

namespace IDPAMatchPrep
{
    public class IDPABaseShooter
    {
        public string firstName;
        public string lastName;
        public string memberNumber;
        public string shooterDivision;
        public string shooterClass;
    }

    public class IDPAWebShooterDetails : IDPABaseShooter
    {
        public string status;
        public DateTime ExpDate;
        public string state;
        public string classCDP;
        public string classESP;
        public string classSSP;
        public string classCCP;
        public string classREV;
        public string classBUG;
        public DateTime lastCDP;
        public DateTime lastESP;
        public DateTime lastSSP;
        public DateTime lastCCP;
        public DateTime lastREV;
        public DateTime lastBUG;
    }
}
