using System;
using System.Collections.Generic;
using System.Text;

namespace TCPark_PDA
{
    public class KeyBoardHelper
    {
        private bool _isBold;
        public bool isBoldFont
        {
            get { return _isBold; }
        }

        private int _maxLength;
        public int nMaxLength
        {
            get { return _maxLength; }
        }

        private string _cantInputString;
        public string CantInputString
        {
            get { return _cantInputString; }
        }

        private string _okString;
        public string okString
        {
            get { return _okString; }
        }


        public delegate bool CheckResult(string resultString);

        private CheckResult _checkMethod;
        public CheckResult CheckMethod
        {
            get { return _checkMethod; }
        }

        private CheckResult _CheckInput;
        public CheckResult CheckInput
        {
            set { _CheckInput = value; }
            get { return _CheckInput; }
        }

        public KeyBoardHelper(int maxLength, string prohibitString, string okString, CheckResult checkMethod)
            : this(maxLength, prohibitString, okString, checkMethod, true)
        {

        }

        public KeyBoardHelper(int maxLength, string prohibitString, string okString, CheckResult checkMethod, bool isBold)
        {
            _maxLength = maxLength;
            _cantInputString = prohibitString;
            _checkMethod = checkMethod;
            _okString = okString;
            _isBold = isBold;
        }
    }
}
