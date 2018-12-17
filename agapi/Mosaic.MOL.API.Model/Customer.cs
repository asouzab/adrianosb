namespace Mosaic.MOL.API.Model
{
    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CpfCnpj { get; set; }
        public string RgIe { get; set; }
        public Address Address { get; set; }

        public string TrimmedId
        {
            get
            {
                return this.Id.TrimStart('0');
            }
        }

        public string TrimmedIdAndName
        {
            get
            {
                return TrimmedId + " - " + Name;
            }
        }
    }
}
