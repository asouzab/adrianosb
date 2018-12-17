using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class SecurityController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        [HttpGet]
        public List<MenuItem> GetMenu(string appSymbol, int userId)
        {
            List<MenuItem> items;
            if (MemoryCacher.GetValue("menu_" + userId.ToString()) == null)
            {
                SecurityDAO securityDAO = new SecurityDAO(connectionString);
                items = securityDAO.GetMenu(appSymbol, userId, "");
                MemoryCacher.Add("menu_" + userId.ToString(), items, new DateTimeOffset(DateTime.Now.AddHours(6)));
            }
            else
            {
                items = (List<MenuItem>)MemoryCacher.GetValue("menu_" + userId.ToString());
            }
            return items;
        }
    }

    public static class MemoryCacher
    {
        public static object GetValue(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Get(key);
        }

        public static bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Add(key, value, absExpiration);
        }

        public static void Delete(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
            {
                memoryCache.Remove(key);
            }
        }
    }
}
