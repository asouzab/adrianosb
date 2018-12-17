using System;

namespace Mosaic.MOL.API.Model
{
    public class Plant
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string CompanyCode { get; set; }


        public string IdAndDescription
        {
            get
            {
                return String.Format("{0} - {1}", Id, Description);
            }
        }
    }
}
