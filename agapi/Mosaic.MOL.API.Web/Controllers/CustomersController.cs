using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class CustomersController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        [HttpGet]
        public IEnumerable<Customer> Search(string salesOrganizationId, string query)
        {
            return new CustomerDAO(connectionString).SearchBySalesOrganization(salesOrganizationId, query);
        }
    }
}
