using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace WBeParkingPDA
{
    public class SyncDataViewModel
    {
        public SyncDataViewModel()
        {
            ETCBinding = new Collection<ETCBinding>();
            CarPurposeTypes = new Collection<CarPurposeTypes>();
        }
        public virtual ICollection<ETCBinding> ETCBinding { get; set; }

        public virtual ICollection<CarPurposeTypes> CarPurposeTypes { get; set; }
    }
}
