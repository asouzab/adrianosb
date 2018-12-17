using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mosaic.MOL.API.DAL
{
    public class SalesOrderDAO
    {
        private string connString;

        public SalesOrderDAO(string connString)
        {
            this.connString = connString;
        }

        public IEnumerable<SalesOrder> SalesOrdersFromContractItem(string contractNumber, int contractItemId)
        {
            IEnumerable<SalesOrder> salesOrders = new List<SalesOrder>();
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_nu_contrato_sap", value: contractNumber, dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_item_contrato", value: contractItemId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var lookup = new Dictionary<string, SalesOrder>();
                connection.Query<SalesOrder, SalesOrderItem, Material, SalesOrder>(
                    "vnd.gx_customer_portal.px_contract_item_sales_orders",
                    (so, it, ma) =>
                    {
                        SalesOrder salesOrder;
                        if (!lookup.TryGetValue(so.Id, out salesOrder))
                        {
                            lookup.Add(so.Id, salesOrder = so);
                        }
                        if (salesOrder.SalesOrderItems == null)
                        {
                            salesOrder.SalesOrderItems = new List<SalesOrderItem>();
                        }
                        it.Material = ma;
                        salesOrder.SalesOrderItems.Add(it);
                        return salesOrder;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                ).AsQueryable();
                salesOrders = lookup.Values;
            }
            return salesOrders;
        }
    }
}
