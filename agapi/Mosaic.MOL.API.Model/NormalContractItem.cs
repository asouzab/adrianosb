using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class NormalContractItem : ContractItem
    {
        public List<SalesOrder> SalesOrders { get; set; }

        public NormalContractItem()
        {
            SalesOrders = new List<SalesOrder>();
        }
    }
}
