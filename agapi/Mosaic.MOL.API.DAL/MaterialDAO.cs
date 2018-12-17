using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mosaic.MOL.API.DAL
{
    public class MaterialDAO
    {
        private String connString;

        public MaterialDAO(string connString)
        {
            this.connString = connString;
        }


        public IEnumerable<Material> Search(string query, decimal n, decimal p, decimal k, string salesOrganizationId, string distributionChannelId)
        {
            IEnumerable<Material> result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_n", value: n, dbType: OracleDbType.Decimal, direction: ParameterDirection.Input);
                parameters.Add("p_p", value: p, dbType: OracleDbType.Decimal, direction: ParameterDirection.Input);
                parameters.Add("p_k", value: k, dbType: OracleDbType.Decimal, direction: ParameterDirection.Input);
                parameters.Add("p_query", value: query, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_org", value: salesOrganizationId, dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_distribution_channel", value: distributionChannelId, dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                result = connection.Query<Material, SalesOrganization, DistributionChannel, Material>(
                    "vnd.gx_contract_master.px_material_search",
                    (material, salesOrganization, distributionChannel) =>
                    {
                        material.SalesOrganization = salesOrganization;
                        material.DistributionChannel = distributionChannel;
                        return material;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            return result;
        }
    }
}
