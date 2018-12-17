using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class UsersController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        [HttpGet]
        public IEnumerable<User> ListByType(int type)
        {
            return new UserDAO(connectionString).ListByType(type);
        }

        [HttpGet]
        public IEnumerable<User> QueryByType(int type, string query)
        {
            return new UserDAO(connectionString).QueryByType(type, query);
        }

    }
}
