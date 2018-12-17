using System;

namespace Mosaic.MOL.API.Model
{
    public class Period
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public string Key { get; set; }
        public User CreationUser { get; set; }
        public User ModifyUser { get; set; }
        public string MasterContractItemId { get; set; }

        public long UnixDate {
            get
            {
                DateTime zeroedDate = new DateTime(Date.Year, Date.Month, Date.Day, 0, 0, 0);
                long unixTime = ((DateTimeOffset)zeroedDate).ToUnixTimeSeconds() * 1000;
                return unixTime;
            }
        }

        public string UID
        {
            get
            {
                if (string.IsNullOrEmpty(MasterContractItemId))
                {
                    return "";
                }
                string uid = MasterContractItemId.ToString().PadLeft(9, '0') + UnixDate.ToString();
                return uid;
            }
        }
    }
}
