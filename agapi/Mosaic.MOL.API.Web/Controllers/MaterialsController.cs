using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class MaterialsController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        [HttpGet]
        public IEnumerable<Material> Search(string query, decimal n, decimal p, decimal k, string salesOrganizationId, string distributionChannelId)
        {
            return new MaterialDAO(connectionString).Search(query, n, p, k, salesOrganizationId, distributionChannelId);
        }
    }
}
