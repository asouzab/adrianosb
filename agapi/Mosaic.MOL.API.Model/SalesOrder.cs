using System;
using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class SalesOrder
    {
        public string Id { get; set; }
        public DateTime? PODate { get; set; }
        public List<SalesOrderItem> SalesOrderItems { get; set; }

        public SalesOrder()
        {
            SalesOrderItems = new List<SalesOrderItem>();
        }

        public string TrimmedId
        {
            get
            {
                return this.Id.TrimStart('0');
            }
        }

        public string PODate_S
        {
            get
            {
                return PODate == null ? "" : PODate.Value.ToString("dd/MM/yyyy");
            }
        }
    }
}
