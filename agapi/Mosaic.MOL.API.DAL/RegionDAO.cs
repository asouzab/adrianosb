using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;

namespace Mosaic.MOL.API.DAL
{
    public class RegionDAO
    {
        private string connString;

        public RegionDAO(string connString)
        {
            this.connString = connString;
        }

        public IEnumerable<Region> Search(string query)
        {
            IEnumerable<Region> result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_query", value: query, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                result = connection.Query<Region>(
                    "vnd.gx_contract_master.px_region",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            return result;
        }
    }
}
