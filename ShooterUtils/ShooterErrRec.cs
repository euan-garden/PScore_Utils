using System;
using System.Collections.Generic;
using System.Text;

namespace ShooterUtils
{
    public class ShooterErrorRec
    {
        public IDPABaseShooter baseShooter;
        public IDPAWebShooterDetails webShooter;
        public string errorMsg;
        public ErrorClass eClass;
    }

    public enum ErrorClass
    {
        Number,
        Name
    };
}
