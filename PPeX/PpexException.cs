using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPeX
{
    public class PpexException : Exception
    {
        public enum PpexErrorCode
        {
            FileNotPPXArchive = 1,
            IncorrectVersionNumber = 2,
        }

        public PpexErrorCode ErrorCode;

        public PpexException(string message, PpexErrorCode errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
