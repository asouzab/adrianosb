using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class SalesOrganizationsController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        [HttpGet]
        public IEnumerable<SalesOrganization> GetAll()
        {
            return new SalesOrganizationDAO(connectionString).All();
        }
    }
}
