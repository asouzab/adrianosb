using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;

namespace Mosaic.MOL.API.DAL
{
    public class PlantDAO
    {
        private string connString;

        public PlantDAO(string connString)
        {
            this.connString = connString;
        }

        public IEnumerable<Plant> ListByCompanyCode(string companyCode)
        {
            IEnumerable<Plant> plants;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_company_code", value: companyCode, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_retorno", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                plants = connection.Query<Plant>(
                    "vnd.gx_pricelist.px_plants",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            return plants;
        }
    }
}
