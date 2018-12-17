using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class SalesOrdersController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        [Route("api/salesorders/fromcontractitem")]
        [HttpGet]
        public IEnumerable<SalesOrder> SalesOrdersFromContractItem(string contractNumber, int contractItemId)
        {
            SalesOrderDAO salesOrderDAO = new SalesOrderDAO(connectionString);
            IEnumerable<SalesOrder> salesOrders = salesOrderDAO.SalesOrdersFromContractItem(contractNumber, contractItemId);
            return salesOrders;
        }
    }
}
