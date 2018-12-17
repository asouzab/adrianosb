using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mosaic.MOL.API.DAL
{
    public class ContractDAO
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string connString;

        public ContractDAO(string connString)
        {
            this.connString = connString;
        }


        public MasterContract InsertMasterContract(MasterContract contract)
        {
            MasterContract result = null;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_tipo_contrato", value: (contract.DocumentType == null ? "" : contract.DocumentType.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_org", value: (contract.SalesOrganization == null ? "" : contract.SalesOrganization.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_distribution_channel", value: (contract.DistributionChannel == null ? "" : contract.DistributionChannel.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_division", value: contract.SalesDivision.Id, dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_group", value: (contract.SalesSupervisor == null ? "" : contract.SalesSupervisor.Code), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_cliente", value: (contract.Customer == null ? "" : contract.Customer.Id), dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_dt_inicio", value: contract.StartDate, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_dt_fim", value: contract.EndDate, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_inclusao", value: (contract.CreationUser == null ? 0 : contract.CreationUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_contract_master.pi_contrato_master", parameters, commandType: CommandType.StoredProcedure);
                var lookup = new Dictionary<int, MasterContract>();
                multi.Read(
                    ObjectsForMapping(),
                    (objects) => MapDBFieldsToProperties(objects, lookup)
                );

                if (lookup.Count() > 0)
                {
                    result = lookup.Values.First();
                }
            }
            return result;
        }

        private Type[] ObjectsForMapping()
        {
            return new[]
            {
                typeof(MasterContract),
                typeof(DocumentType),
                typeof(SalesOrganization),
                typeof(DistributionChannel),
                typeof(SalesDivision),
                typeof(User),
                typeof(User),
                typeof(Customer),
                typeof(Address),
                typeof(City),
                typeof(State),
                typeof(User),
                typeof(User),
                typeof(MasterContractItem),
                typeof(Incoterms),
                typeof(Material),
                typeof(Region),
                typeof(User),
                typeof(User),
                typeof(Period),
                typeof(User),
                typeof(User),
                typeof(NormalContract),
                typeof(NormalContractItem),
                typeof(Material)
            };
        }

        private MasterContract MapDBFieldsToProperties(object[] objects, Dictionary<int, MasterContract> lookup)
        {
            MasterContract ct;
            var contract = (MasterContract)objects[0];
            if (!lookup.TryGetValue(contract.Id, out ct))
            {
                lookup.Add(contract.Id, ct = contract);
                if (objects[1] != null)
                {
                    ct.DocumentType = (DocumentType)objects[1];
                }
                if (objects[2] != null)
                {
                    ct.SalesOrganization = (SalesOrganization)objects[2];
                }
                if (objects[3] != null)
                {
                    ct.DistributionChannel = (DistributionChannel)objects[3];
                }
                if (objects[4] != null)
                {
                    ct.SalesDivision = (SalesDivision)objects[4];
                }
                if (objects[5] != null)
                {
                    ct.SalesSupervisor = (User)objects[5];
                }
                if (objects[6] != null)
                {
                    ct.SalesSupervisor.Superior = (User)objects[6];
                }
                if (objects[7] != null)
                {
                    ct.Customer = (Customer)objects[7];
                }
                if (objects[8] != null)
                {
                    ct.Customer.Address = (Address)objects[8];
                }
                if (objects[9] != null)
                {
                    ct.Customer.Address.City = (City)objects[9];
                }
                if (objects[10] != null)
                {
                    ct.Customer.Address.City.State = (State)objects[10];
                }
                if (objects[11] != null)
                {
                    ct.CreationUser = (User)objects[11];
                }
                if (objects[12] != null)
                {
                    ct.ModifyUser = (User)objects[12];
                }
            }
            if (ct.MasterContractItems == null)
            {
                ct.MasterContractItems = new List<MasterContractItem>();
            }

            MasterContractItem masterContractItem;
            var it = (MasterContractItem)objects[13];
            if (it != null)
            {
                var items = from mi in ct.MasterContractItems
                            where mi.Id == it.Id
                            select mi;

                if (items.Count() == 0)
                {
                    masterContractItem = it;
                    masterContractItem.Incoterms = (Incoterms)objects[14];
                    masterContractItem.Material = (Material)objects[15];
                    masterContractItem.Region = (Region)objects[16];
                    masterContractItem.CreationUser = (User)objects[17];
                    masterContractItem.ModifyUser = (User)objects[18];
                    ct.MasterContractItems.Add(masterContractItem);
                }
                else
                {
                    masterContractItem = items.First();
                }
                if (masterContractItem.Periods == null)
                {
                    masterContractItem.Periods = new List<Period>();
                }

                Period period;
                var pe = (Period)objects[19];
                if (pe != null)
                {
                    var periods = from p in masterContractItem.Periods
                                  where p.UID == pe.UID
                                  select p;
                    if (periods.Count() == 0)
                    {
                        period = pe;
                        period.CreationUser = (User)objects[20];
                        period.ModifyUser = (User)objects[21];
                        masterContractItem.Periods.Add(period);
                    }
                }
            }

            NormalContract normalContract;
            var nc = (NormalContract)objects[22];
            if (nc != null)
            {
                var normalContracts = from nct in ct.NormalContracts
                                      where nct.Id == nc.Id
                                      select nct;
                if (normalContracts.Count() == 0)
                {
                    normalContract = nc;
                    ct.NormalContracts.Add(normalContract);
                }
                else
                {
                    normalContract = normalContracts.First();
                }
                if (normalContract.NormalContractItems == null)
                {
                    normalContract.NormalContractItems = new List<NormalContractItem>();
                }

                NormalContractItem normalContractItem;
                var ci = (NormalContractItem)objects[23];
                if (ci != null)
                {
                    var normalContractItems = from i in normalContract.NormalContractItems
                                              where i.Id == ci.Id 
                                              select i;
                    if (normalContractItems.Count() == 0)
                    {
                        normalContractItem = ci;
                        normalContractItem.Material = (Material)objects[24];
                        normalContract.NormalContractItems.Add(normalContractItem);
                    }
                }
            }

            return ct;
        }

        public MasterContract UpdateMasterContract(MasterContract contract)
        {
            MasterContract result = null;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato_master", value: contract.Id, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_tipo_contrato", value: (contract.DocumentType == null ? "" : contract.DocumentType.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_org", value: (contract.SalesOrganization == null ? "" : contract.SalesOrganization.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_distribution_channel", value: (contract.DistributionChannel == null ? "" : contract.DistributionChannel.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_division", value: contract.SalesDivision.Id, dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_group", value: (contract.SalesSupervisor == null ? "" : contract.SalesSupervisor.Code), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_cliente", value: (contract.Customer == null ? "" : contract.Customer.Id), dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_dt_inicio", value: contract.StartDate, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_dt_fim", value: contract.EndDate, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_alteracao", value: (contract.ModifyUser == null ? 0 : contract.ModifyUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_contract_master.pu_contrato_master", parameters, commandType: CommandType.StoredProcedure);
                var lookup = new Dictionary<int, MasterContract>();
                multi.Read(
                    ObjectsForMapping(),
                    (objects) => MapDBFieldsToProperties(objects, lookup)
                );

                if (lookup.Count() > 0)
                {
                    result = lookup.Values.First();
                }
            }
            return result;
        }

        public MasterContract GetMasterContract(int contractId)
        {
            MasterContract result = null;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato_master", value: contractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_contract_master.px_contrato_master", param: parameters, commandType: CommandType.StoredProcedure);
                var lookup = new Dictionary<int, MasterContract>();

                multi.Read(
                    ObjectsForMapping(),
                    (objects) => MapDBFieldsToProperties(objects, lookup)
                );

                if (lookup.Count() > 0)
                {
                    result = lookup.Values.First();
                }
            }
            return result;
        }

        public NormalContract GenerateNormalContract(NormalContract normalContract)
        {
            NormalContract result = null;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();

                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato_master", value: normalContract.MasterContractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_tipo_contrato", value: (normalContract.DocumentType == null ? "" : normalContract.DocumentType.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_org", value: (normalContract.SalesOrganization == null ? "" : normalContract.SalesOrganization.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_distribution_channel", value: (normalContract.DistributionChannel == null ? "" : normalContract.DistributionChannel.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_sales_division", value: normalContract.SalesDivision.Id, dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_venda", value: (normalContract.SalesSupervisor == null ? null : normalContract.SalesSupervisor.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_cliente", value: (normalContract.Customer == null ? "" : normalContract.Customer.Id), dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_dt_inicio", value: normalContract.StartDate, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_dt_fim", value: normalContract.EndDate, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_inclusao", value: (normalContract.CreationUser == null ? null : normalContract.CreationUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_contract_master.pu_generate_contract", param: parameters, commandType: CommandType.StoredProcedure);

                IEnumerable<NormalContract> list = multi.Read(
                new[]
                {
                    typeof(NormalContract),
                    typeof(DocumentType),
                    typeof(SalesOrganization),
                    typeof(DistributionChannel),
                    typeof(SalesDivision),
                    typeof(User),
                    typeof(User),
                    typeof(Customer),
                    typeof(User),
                    typeof(NormalContractItem),
                    //typeof(Incoterms),
                    typeof(Material)
                },
                (objects) =>
                {
                    var lookup = new Dictionary<int, NormalContract>();
                    NormalContract ct;
                    var contract = (NormalContract)objects[0];
                    if (!lookup.TryGetValue(contract.Id, out ct))
                    {
                        lookup.Add(contract.Id, ct = contract);
                        ct.DocumentType = (DocumentType)objects[1];
                        ct.SalesOrganization = (SalesOrganization)objects[2];
                        ct.DistributionChannel = (DistributionChannel)objects[3];
                        ct.SalesDivision = (SalesDivision)objects[4];
                        ct.SalesSupervisor = (User)objects[5];
                        ct.SalesSupervisor.Superior = (User)objects[6];
                        ct.Customer = (Customer)objects[7];
                        ct.CreationUser = (User)objects[8];
                    }
                    if (ct.NormalContractItems == null)
                    {
                        ct.NormalContractItems = new List<NormalContractItem>();
                    }

                    NormalContractItem normalContractItem;
                    var it = (NormalContractItem)objects[9];
                    if (it != null)
                    {
                        var items = from mi in ct.NormalContractItems
                                    where mi.Id == it.Id
                                    select mi;

                        if (items.Count() == 0)
                        {
                            normalContractItem = it;
                            normalContractItem.Material = (Material)objects[10];
                            //normalContractItem.Incoterms = (Incoterms)objects[11];
                            ct.NormalContractItems.Add(normalContractItem);
                        }
                        else
                        {
                            normalContractItem = items.First();
                        }
                    }
                    return ct;
                }).AsList();

                if (list.Count() > 0)
                {
                    result = list.First();
                }
            }
            return result;
        }


        public NormalContractItem InsertNormalContractItem(NormalContractItem normalContractItem, DateTime deliveryDate, User creationUser)
        {
            NormalContractItem result = null;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();

                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato", value: normalContractItem.ContractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_produto_sap", value: (normalContractItem.Material == null ? "" : normalContractItem.Material.Id), dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_nu_quantidade", value: normalContractItem.Quantity, dbType: OracleDbType.Decimal, direction: ParameterDirection.Input);
                parameters.Add("p_dh_remessa", value: deliveryDate, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_inclusao", value: (creationUser == null ? null : creationUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var list = connection.Query<NormalContractItem, Material, NormalContractItem>(
                    "vnd.gx_contract_master.pi_normal_contract_item",
                    (item, material) =>
                    {
                        item.Material = material;
                        return item;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (list.Count() > 0)
                {
                    result = list.First();
                }

                return result;
            }
        }


        public Result SetNormalContractSentToSAP(NormalContract normalContract, string SAPNumber)
        {
            Result result = new Result(success: true);
            try
            {
                using (IDbConnection connection = new OracleConnection(this.connString))
                {
                    connection.Open();

                    var parameters = new OracleDynamicParameters();
                    parameters.Add("p_cd_contrato", value: normalContract.Id, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                    parameters.Add("p_nu_contrato_sap", value: SAPNumber, dbType: OracleDbType.Char, direction: ParameterDirection.Input);

                    connection.Execute("vnd.gx_contract_master.pu_set_normal_sent_to_sap", param: parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Messages.Add(ex.Message);
            }
            return result;
        }


        public Result SetMasterContractSentToSAP(MasterContract masterContract)
        {
            Result result = new Result(success: true);
            try
            {
                using (IDbConnection connection = new OracleConnection(this.connString))
                {
                    connection.Open();

                    var parameters = new OracleDynamicParameters();
                    parameters.Add("p_cd_contrato_master", value: masterContract.Id, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                    parameters.Add("p_cd_usuario", value: (masterContract.GeneratorUser == null ? 0 : masterContract.GeneratorUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);

                    connection.Execute("vnd.gx_contract_master.pu_set_master_sent_to_sap", param: parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Messages.Add(ex.Message);
            }
            return result;
        }


        public Result SetQuantityOfContractsInMasterContract(int masterContractId, int quantityOfContracts)
        {
            Result result = new Result(success: true);
            try
            {
                using (IDbConnection connection = new OracleConnection(this.connString))
                {
                    connection.Open();

                    var parameters = new OracleDynamicParameters();
                    parameters.Add("p_cd_contrato_master", value: masterContractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                    parameters.Add("p_qt_contratos", value: quantityOfContracts, dbType: OracleDbType.Decimal, direction: ParameterDirection.Input);

                    connection.Execute("vnd.gx_contract_master.pu_master_qt_contratos", param: parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Messages.Add(ex.Message);
            }
            return result;
        }


        public NormalContract GetNormalContractWithMaster(int normalContractId)
        {
            NormalContract result = null;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();

                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato", value: normalContractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_contract_master.px_normal_contract_with_master", param: parameters, commandType: CommandType.StoredProcedure);

                IEnumerable<NormalContract> list = multi.Read(
                new[]
                {
                    typeof(NormalContract),
                    typeof(DocumentType),
                    typeof(SalesOrganization),
                    typeof(DistributionChannel),
                    typeof(SalesDivision),
                    typeof(User),
                    typeof(User),
                    typeof(Customer),
                    typeof(User),
                    typeof(NormalContractItem),
                    typeof(Material),
                    typeof(Incoterms),
                    typeof(Region)
                },
                (objects) =>
                {
                    var lookup = new Dictionary<int, NormalContract>();
                    NormalContract ct;
                    var contract = (NormalContract)objects[0];
                    if (!lookup.TryGetValue(contract.Id, out ct))
                    {
                        lookup.Add(contract.Id, ct = contract);
                        ct.DocumentType = (DocumentType)objects[1];
                        ct.SalesOrganization = (SalesOrganization)objects[2];
                        ct.DistributionChannel = (DistributionChannel)objects[3];
                        ct.SalesDivision = (SalesDivision)objects[4];
                        ct.SalesSupervisor = (User)objects[5];
                        ct.SalesSupervisor.Superior = (User)objects[6];
                        ct.Customer = (Customer)objects[7];
                        ct.CreationUser = (User)objects[8];
                    }
                    if (ct.NormalContractItems == null)
                    {
                        ct.NormalContractItems = new List<NormalContractItem>();
                    }

                    NormalContractItem normalContractItem;
                    var it = (NormalContractItem)objects[9];
                    if (it != null)
                    {
                        var items = from mi in ct.NormalContractItems
                                    where mi.Id == it.Id
                                    select mi;

                        if (items.Count() == 0)
                        {
                            normalContractItem = it;
                            normalContractItem.Material = (Material)objects[10];
                            normalContractItem.Incoterms = (Incoterms)objects[11];
                            normalContractItem.Region = (Region)objects[12];
                            ct.NormalContractItems.Add(normalContractItem);
                        }
                        else
                        {
                            normalContractItem = items.First();
                        }
                    }
                    return ct;
                }).AsList();

                if (list.Count() > 0)
                {
                    result = list.First();
                }
            }
            return result;
        }


        public SearchResult SearchMasterContracts(int salesUserId, int pageSize, int page)
        {
            SearchResult result = new SearchResult();
            result.TotalRecords = 0;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_usuario_venda", value: salesUserId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_page_size", value: pageSize, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_page", value: page, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                parameters.Add("p_result_total", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_contract_master.px_search_contrato_master", parameters, commandType: CommandType.StoredProcedure);
                result.Payload = multi.Read(
                    new[]
                    {
                        typeof(MasterContract),
                        typeof(DocumentType),
                        typeof(SalesOrganization),
                        typeof(DistributionChannel),
                        typeof(SalesDivision),
                        typeof(User),
                        typeof(User),
                        typeof(Customer),
                        typeof(User),
                        typeof(User)
                    },
                    (objects) =>
                    {
                        var contract = (MasterContract)objects[0];
                        contract.DocumentType = (DocumentType)objects[1];
                        contract.SalesOrganization = (SalesOrganization)objects[2];
                        contract.DistributionChannel = (DistributionChannel)objects[3];
                        contract.SalesDivision = (SalesDivision)objects[4];
                        contract.SalesSupervisor = (User)objects[5];
                        contract.SalesSupervisor.Superior = (User)objects[6];
                        contract.Customer = (Customer)objects[7];
                        contract.CreationUser = (User)objects[8];
                        contract.ModifyUser = (User)objects[9];
                        return contract;
                    }
                ).AsList();

                var row = (IDictionary<string, object>)multi.ReadFirst();
                if (row["TOTAL"] != null)
                {
                    int total;
                    int.TryParse(row["TOTAL"].ToString(), out total);
                    result.TotalRecords = total;
                }
            }
            return result;
        }

        public IEnumerable<NormalContract> LastestFromCustomer(int userId, int quantity)
        {
            List<NormalContract> normalContracts;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_usuario", value: userId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_quantity", value: quantity, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                normalContracts = connection.Query<NormalContract, Customer, NormalContract>(
                    "vnd.gx_customer_portal.px_latest_contracts",
                    (contract, customer) =>
                    {
                        contract.Customer = customer;
                        return contract;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                ).AsList();
            }
            return normalContracts;
        }


        public IEnumerable<NormalContractItem> DocumentFlow(int contractId)
        {
            IEnumerable<NormalContractItem> contractItems = new List<NormalContractItem>();
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato", value: contractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var lookup = new Dictionary<int, NormalContractItem>();
                connection.Query<NormalContractItem, Material, SalesOrder, SalesOrderItem, NotaFiscal, NotaFiscalItem, NormalContractItem>(
                    "vnd.gx_customer_portal.px_document_flow",
                    (ci, ma, so, soi, nf, nfi) =>
                    {
                        NormalContractItem normalContractItem;
                        if (!lookup.TryGetValue(ci.Id, out normalContractItem))
                        {
                            lookup.Add(ci.Id, normalContractItem = ci);
                        }

                        if (normalContractItem.SalesOrders == null)
                        {
                            normalContractItem.SalesOrders = new List<SalesOrder>();
                        }

                        normalContractItem.Material = ma;

                        SalesOrder salesOrder;
                        if (so != null)
                        {
                            var salesOrders = from _so in normalContractItem.SalesOrders
                                              where _so.Id == so.Id
                                              select _so;
                            if (salesOrders.Count() == 0)
                            {
                                salesOrder = so;
                                normalContractItem.SalesOrders.Add(salesOrder);
                            }
                            else
                            {
                                salesOrder = salesOrders.First();
                            }
                            if (salesOrder.SalesOrderItems == null)
                            {
                                salesOrder.SalesOrderItems = new List<SalesOrderItem>();
                            }

                            SalesOrderItem salesOrderItem;
                            if(soi != null)
                            {
                                var salesOrderItems = from _soi in salesOrder.SalesOrderItems
                                                      where _soi.Id == soi.Id
                                                      select _soi;
                                if (salesOrderItems.Count() == 0)
                                {
                                    salesOrderItem = soi;
                                    salesOrder.SalesOrderItems.Add(salesOrderItem);
                                }
                                else
                                {
                                    salesOrderItem = salesOrderItems.First();
                                }

                                NotaFiscal notaFiscal;
                                if (nf != null)
                                {
                                    var notaFiscals = from _nf in salesOrderItem.NotaFiscals
                                                      where _nf.Id == nf.Id
                                                      select _nf;
                                    if (notaFiscals.Count() == 0)
                                    {
                                        notaFiscal = nf;
                                        salesOrderItem.NotaFiscals.Add(notaFiscal);
                                    }
                                    else
                                    {
                                        notaFiscal = notaFiscals.First();
                                    }

                                    NotaFiscalItem notaFiscalItem;
                                    if (nfi != null)
                                    {
                                        var notaFiscalItems = from _nfi in notaFiscal.NotaFiscalItems
                                                              where _nfi.Id == nfi.Id
                                                              select _nfi;
                                        if (notaFiscalItems.Count() == 0)
                                        {
                                            notaFiscalItem = nfi;
                                            notaFiscal.NotaFiscalItems.Add(notaFiscalItem);
                                        }
                                        else
                                        {
                                            // Nota Fiscal Item is the leaf node.
                                            // We could suppress this, unless we add a child node to it.
                                            notaFiscalItem = notaFiscalItems.First();
                                        }
                                    }
                                }
                            }
                        }

                        return normalContractItem;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                ).AsQueryable();
                contractItems = lookup.Values;
            }
            return contractItems;
        }


        public SearchResult SearchContractsFromCustomer(int userId, string term, int pageSize, int page)
        {
            SearchResult result = new SearchResult();
            result.TotalRecords = 0;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_usuario", value: userId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_term", value: term, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                parameters.Add("p_page_size", value: pageSize, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_page", value: page, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                parameters.Add("p_result_total", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_customer_portal.px_search_contracts", parameters, commandType: CommandType.StoredProcedure);

                // Read first cursor.
                result.Payload = multi.Read<NormalContract, Customer, NormalContract>(
                    (contract, customer) =>
                    {
                        contract.Customer = customer;
                        return contract;
                    }
                ).AsList();

                // Read second cursor.
                var row = (IDictionary<string, object>)multi.ReadFirst();
                if (row["TOTAL"] != null)
                {
                    int total;
                    int.TryParse(row["TOTAL"].ToString(), out total);
                    result.TotalRecords = total;
                }
            }
            return result;
        }


        public MasterContract ResetMasterContract(int masterContractId, int userId)
        {
            MasterContract result = null;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato_master", value: masterContractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario", value: userId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_contract_master.pu_reset_contrato_master", param: parameters, commandType: CommandType.StoredProcedure);
                var lookup = new Dictionary<int, MasterContract>();

                multi.Read(
                    ObjectsForMapping(),
                    (objects) => MapDBFieldsToProperties(objects, lookup)
                );

                if (lookup.Count() > 0)
                {
                    result = lookup.Values.First();
                }
            }
            return result;
        }


        public MasterContract InactivateMasterContract(int masterContractId, int userId)
        {
            MasterContract result = null;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato_master", value: masterContractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario", value: userId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                var multi = connection.QueryMultiple("vnd.gx_contract_master.pu_inactivate_contrato_master", param: parameters, commandType: CommandType.StoredProcedure);
                var lookup = new Dictionary<int, MasterContract>();

                multi.Read(
                    ObjectsForMapping(),
                    (objects) => MapDBFieldsToProperties(objects, lookup)
                );

                if (lookup.Count() > 0)
                {
                    result = lookup.Values.First();
                }
            }
            return result;
        }
    }
}
