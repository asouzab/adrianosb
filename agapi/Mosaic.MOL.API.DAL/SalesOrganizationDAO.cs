using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mosaic.MOL.API.DAL
{
    public class SalesOrganizationDAO
    {
        private String connString;

        public SalesOrganizationDAO(string connString)
        {
            this.connString = connString;
        }


        public IEnumerable<SalesOrganization> All()
        {
            IEnumerable<SalesOrganization> result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                result = connection.Query<SalesOrganization>(@"select cd_sales_org as Id,
                    no_sales_org as Description
                    from vnd.sales_org 
                    where ic_master_contract = 'S'",
                    commandType: CommandType.Text
                    );
            }
            return result;
        }
    }
}
