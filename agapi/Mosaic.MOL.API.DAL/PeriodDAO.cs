using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Mosaic.MOL.API.DAL
{
    public class PeriodDAO
    {
        private string connString;

        public PeriodDAO(string connString)
        {
            this.connString = connString;
        }

        public Period InsertPeriod(int contractId, int contractMasterItemId, Period period)
        {
            Period result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();

                parameters.Add("p_cd_contrato_master", value: contractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_contrato_master_item", value:contractMasterItemId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_dt_cadencia", value: period.Date, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_nu_quantidade", value: period.Quantity, dbType: OracleDbType.Decimal, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_inclusao", value: (period.CreationUser == null ? 0 : period.CreationUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                result = connection.Query<Period, User, User, Period>(
                    "vnd.gx_contract_master.pi_contrato_master_item_caden",
                    (per, creationUser, modifyUser) =>
                    {
                        per.CreationUser = creationUser;
                        per.ModifyUser = modifyUser;
                        return per;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                ).AsList<Period>().First();
            }
            return result;
        }

        public Period UpdatePeriod(Period period)
        {
            Period result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();

                parameters.Add("p_cd_contrato_master_item_cad", value: period.Id, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_dt_cadencia", value: period.Date, dbType: OracleDbType.Date, direction: ParameterDirection.Input);
                parameters.Add("p_nu_quantidade", value: period.Quantity, dbType: OracleDbType.Decimal, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_alteracao", value: (period.ModifyUser == null ? 0 : period.ModifyUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                result = connection.Query<Period, User, User, Period>(
                    "vnd.gx_contract_master.pu_contrato_master_item_caden",
                    (per, creationUser, modifyUser) =>
                    {
                        per.CreationUser = creationUser;
                        per.ModifyUser = modifyUser;
                        return per;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                ).AsList<Period>().First();
            }
            return result;
        }

        public IEnumerable<Period> ListByContract(int contractId)
        {
            IEnumerable<Period> periods;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato_master", value: contractId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                periods = connection.Query<Period, User, User, Period>(
                    "vnd.gx_contract_master.px_cadencias", 
                    (per, creationUser, modifyUser) =>
                    {
                        per.CreationUser = creationUser;
                        per.ModifyUser = modifyUser;
                        return per;
                    },
                    param: parameters, 
                    commandType: CommandType.StoredProcedure
                );
            }
            return periods;
        }

        public bool Delete(Period period)
        {
            bool success = false;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_contrato_master_item_cad", value: period.Id, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_cd_usuario_alteracao", value: (period.ModifyUser == null ? 0 : period.ModifyUser.Id), dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                var n = connection.ExecuteScalar("vnd.gx_contract_master.pd_contrato_master_item_caden", parameters, commandType: CommandType.StoredProcedure);
                int recordsAffected = Convert.ToInt32(n);
                success = recordsAffected > 0;
            }
            return success;
        }
    }
}
