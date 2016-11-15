using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WBeParkingPDA
{
    public class ETCBinding
    {
        public string ETCID { get; set; }
        public string CarID { get; set; }
        public Nullable<int> CarPurposeTypeID { get; set; }
    }
}
