using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mosaic.MOL.API.DAL
{
    public class CustomerDAO
    {
        private String connString;

        public CustomerDAO(string connString)
        {
            this.connString = connString;
        }

        public IEnumerable<Customer> SearchBySalesOrganization(string salesOrganizationId, string query)
        {
            IEnumerable<Customer> result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_sales_org", value: salesOrganizationId, dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_query", value: query, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                result = connection.Query<Customer, Address, City, State, Customer>(
                    "vnd.gx_contract_master.px_customer", 
                    (customer, address, city, state) =>
                    {
                        city.State = state;
                        address.City = city;
                        customer.Address = address;
                        return customer;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            return result;
        }
    }
}
