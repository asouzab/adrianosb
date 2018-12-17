using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class NormalContract : Contract
    {
        public long PONumber { get; set; }
        public int MasterContractId { get; set; }
        public List<NormalContractItem> NormalContractItems { get; set; }
        public bool Success { get; set; }

        public NormalContract()
        {
            this.Success = true;
            this.NormalContractItems = new List<NormalContractItem>();
        }
    }
}
