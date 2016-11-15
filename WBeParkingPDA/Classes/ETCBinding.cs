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
        public DateTime CreateTime { get; set; }
        public DateTime? LastUpdateTiem { get; set; }
        public DateTime? LastUploadTime { get; set; }
    }
}
