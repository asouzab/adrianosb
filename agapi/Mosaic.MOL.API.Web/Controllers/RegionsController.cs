using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class RegionsController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        [HttpGet]
        public IEnumerable<Region> Search(string query)
        {
            IEnumerable<Region> regions = new RegionDAO(connectionString).Search(query);
            return regions;
        }
    }
}
