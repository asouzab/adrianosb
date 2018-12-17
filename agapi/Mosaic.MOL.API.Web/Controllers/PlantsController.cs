using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class PlantsController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        [Route("api/plants/fromcompany")]
        [HttpGet]
        public IEnumerable<Plant> SalesOrdersFromContractItem(string companyCode)
        {
            PlantDAO plantDAO = new PlantDAO(connectionString);
            IEnumerable<Plant> plants = plantDAO.ListByCompanyCode(companyCode);
            return plants;
        }
    }
}
