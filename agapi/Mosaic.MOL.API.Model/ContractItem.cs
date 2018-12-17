namespace Mosaic.MOL.API.Model
{
    public abstract class ContractItem
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public decimal Quantity { get; set; }
        public Region Region { get; set; }
        public Incoterms Incoterms { get; set; }
        public Material Material { get; set; }


        public string Quantity_S
        {
            get
            {
                return Quantity.ToString("#,##0.000");
            }
        }
    }
}
