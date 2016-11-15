using System;
using System.Collections.Generic;
using System.Text;

namespace TCPark_PDA
{
    enum KeyBoardInputType
    {
        Password,
        Mac,
        CarNo,
        TicketNo,
        CarNoWithOutCheck
    }

    class KeyBoardHelperFactory
    {
        const int CarNoMaxLength = 8;

        public static KeyBoardHelper MakeKeyBoardHelper(KeyBoardInputType type)
        {
            switch (type)
            {
                case KeyBoardInputType.Password: return PwdHelper();
                case KeyBoardInputType.Mac: return MacHelper();
                case KeyBoardInputType.CarNo: return CarNoHelper();
                case KeyBoardInputType.TicketNo: return TicketNoHelper();
                case KeyBoardInputType.CarNoWithOutCheck: return CarNoWithOutCheckHelper();
                default: return PwdHelper();
            }
        }

        private static KeyBoardHelper CarNoWithOutCheckHelper()
        {
            KeyBoardHelper helper = new KeyBoardHelper(CarNoMaxLength, @"*#", string.Empty, new KeyBoardHelper.CheckResult(NoCheck));
            helper.CheckInput = new KeyBoardHelper.CheckResult(CheckInputCarNo);
            
            return helper;
        }

        private static KeyBoardHelper TicketNoHelper()
        {
            //到時要修改一下
            return new KeyBoardHelper(15, @"-", string.Empty, new KeyBoardHelper.CheckResult(NoCheck), false);
        }

        private static KeyBoardHelper PwdHelper()
        {
            return new KeyBoardHelper(6, string.Empty, string.Empty, new KeyBoardHelper.CheckResult(NoCheck));
        }

        private static bool NoCheck(string source)
        {
            return true;
        }

        private static KeyBoardHelper MacHelper()
        {
            return new KeyBoardHelper(12, string.Empty, @"1234567890ABCDEF", new KeyBoardHelper.CheckResult(FATool.FTChecker.ChkIsMac));
        }

        private static KeyBoardHelper CarNoHelper()
        {
            KeyBoardHelper helper = new KeyBoardHelper(CarNoMaxLength, @"*#", string.Empty, new KeyBoardHelper.CheckResult(FATool.FTChecker.ChkIsCarNo));
            helper.CheckInput = new KeyBoardHelper.CheckResult(CheckInputCarNo);

            return helper;
        }

        private static bool CheckInputCarNo(string NewString)
        {
            int nIndex = NewString.IndexOf("-");
            if (nIndex == -1)
            {
                if (NewString.Length < 5) return true;
                return false;
            }

            if (nIndex <=1) return false;

            string[] strArray = NewString.Split("-".ToCharArray());

            if (strArray.Length > 2) return false;

            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].Length > 4) return false;

                foreach (char c in strArray[i])
                {
                    if (!char.IsLetterOrDigit(c)) return false;
                }
            }

            return true;
        }
    }
}
