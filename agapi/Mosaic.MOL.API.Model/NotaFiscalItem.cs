namespace Mosaic.MOL.API.Model
{
    public class NotaFiscalItem
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Taxes { get; set; }
        public decimal GrossPrice { get; set; }


        public string Quantity_S
        {
            get
            {
                return Quantity.ToString("#,##0.000");
            }
        }

        public string UnitPriceMoneyFormat
        {
            get
            {
                return UnitPrice.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            }
        }

        public string UnitPrice_S
        {
            get
            {
                return string.Format("{0:#,##0.00}", UnitPrice);
            }
        }

        public string NetPriceMoneyFormat
        {
            get
            {
                return NetPrice.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            }
        }

        public string NetPrice_S
        {
            get
            {
                return string.Format("{0:#,##0.00}", NetPrice);
            }
        }

        public string TaxesMoneyFormat
        {
            get
            {
                return Taxes.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            }
        }

        public string Taxes_S
        {
            get
            {
                return string.Format("{0:#,##0.00}", Taxes);
            }
        }

        public string GrossPriceMoneyFormat
        {
            get
            {
                return GrossPrice.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            }
        }

        public string GrossPrice_S
        {
            get
            {
                return string.Format("{0:#,##0.00}", GrossPrice);
            }
        }
    }
}
