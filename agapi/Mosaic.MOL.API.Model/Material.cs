namespace Mosaic.MOL.API.Model
{
    public class Material
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal N { get; set; }
        public decimal P { get; set; }
        public decimal K { get; set; }
        public SalesOrganization SalesOrganization { get; set; }
        public DistributionChannel DistributionChannel { get; set; }

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
                return string.Format("{0} - {1}", Id.TrimStart('0'), Name);
            }
        }
    }
}
