using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mosaic.MOL.API.DAL
{
    public class ContractItemDAO
    {
        private string connString;

        public ContractItemDAO(string connString)
        {
            this.connString = connString;
        }

        public MasterContractItem InsertContractMasterItem(int contractId, MasterContractItem contractMasterItem)
        {
            MasterContractItem result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();

                parameters.Add("p_cd_contrato_master", value: contractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_incoterms", value: (contractMasterItem.Incoterms == null ? "" : contractMasterItem.Incoterms.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_ds_region", value: (contractMasterItem.Region == null ? "" : contractMasterItem.Region.Name), dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_cd_produto_sap", value: (contractMasterItem.Material == null ? "" : contractMasterItem.Material.Id), dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_ic_price_list", value: (contractMasterItem.PriceListOption ? "S" : "N"), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_inclusao", value: (contractMasterItem.CreationUser == null ? 0 : contractMasterItem.CreationUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                result = connection.Query<MasterContractItem, Incoterms, Material, Region, User, User, MasterContractItem>(
                    "vnd.gx_contract_master.pi_contrato_master_item",
                    (item, incoterms, material, region, creationUser, modifyUser) =>
                    {
                        item.Incoterms = incoterms;
                        item.Material = material;
                        item.Region = region;
                        item.CreationUser = creationUser;
                        item.ModifyUser = modifyUser;
                        return item;
                    },
                    param: parameters, 
                    commandType: CommandType.StoredProcedure
                ).AsList<MasterContractItem>().First();
            }

            return result;
        }

        public MasterContractItem UpdateContractMasterItem(MasterContractItem contractMasterItem)
        {
            MasterContractItem result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();

                parameters.Add("p_cd_contrato_master_item", value: contractMasterItem.Id, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_contrato_master", value: contractMasterItem.ContractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_incoterms", value: (contractMasterItem.Incoterms == null ? "" : contractMasterItem.Incoterms.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_ds_region", value: (contractMasterItem.Region == null ? "" : contractMasterItem.Region.Name), dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_cd_produto_sap", value: (contractMasterItem.Material == null ? "" : contractMasterItem.Material.Id), dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_ic_price_list", value: (contractMasterItem.PriceListOption ? "S" : "N"), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_alteracao", value: (contractMasterItem.ModifyUser == null ? 0 : contractMasterItem.ModifyUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                result = connection.Query<MasterContractItem, Incoterms, Material, Region, User, User, MasterContractItem>(
                    "vnd.gx_contract_master.pu_contrato_master_item",
                    (item, incoterms, material, region, creationUser, modifyUser) =>
                    {
                        item.Incoterms = incoterms;
                        item.Material = material;
                        item.Region = region;
                        item.CreationUser = creationUser;
                        item.ModifyUser = modifyUser;
                        return item;
                    },
                    param: parameters, 
                    commandType: CommandType.StoredProcedure
                ).AsList<MasterContractItem>().First();
            }

            return result;
        }

        public IEnumerable<NormalContractItem> NormalContractItemsFromContract(int contractId)
        {
            IEnumerable<NormalContractItem> result;

            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();

                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato", value: contractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                result = connection.Query<NormalContractItem, Material, NormalContractItem>(
                    "vnd.gx_customer_portal.px_items_from_contract", 
                    (item, material) =>
                    {
                        item.Material = material;
                        return item;
                    },
                    param: parameters, 
                    commandType: CommandType.StoredProcedure
                ).AsList();
            }
            return result;
        }
    }
}
