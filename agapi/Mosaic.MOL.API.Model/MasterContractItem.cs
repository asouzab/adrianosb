using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class MasterContractItem : ContractItem
    {
        public string PriceListOptionDb { get; set; }
        public User CreationUser { get; set; }
        public User ModifyUser { get; set; }
        public string Key { get; set; }
        public List<Period> Periods { get; set; }

        public MasterContractItem()
        {
            this.Periods = new List<Period>();
        }

        public bool PriceListOption {
            get
            {
                return string.IsNullOrEmpty(PriceListOptionDb) ? false : (PriceListOptionDb == "S");
            }

            set
            {
                PriceListOptionDb = (value ? "S" : "N");
            }
        }
    }
}
