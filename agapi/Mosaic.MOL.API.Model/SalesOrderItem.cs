using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class SalesOrderItem
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public Material Material { get; set; }
        public List<NotaFiscal> NotaFiscals { get; set; }

        public SalesOrderItem()
        {
            NotaFiscals = new List<NotaFiscal>();
        }

        public string Quantity_S
        {
            get
            {
                return Quantity.ToString("#,##0.000");
            }
        }
    }
}
