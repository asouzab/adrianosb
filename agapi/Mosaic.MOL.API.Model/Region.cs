namespace Mosaic.MOL.API.Model
{
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FederalStateSymbol { get; set; }

        public string NameAndFederalStateSymbol
        {
            get
            {
                return Name + (string.IsNullOrEmpty(FederalStateSymbol) ? "" : " - " + FederalStateSymbol);
            }
        }
    }
}
