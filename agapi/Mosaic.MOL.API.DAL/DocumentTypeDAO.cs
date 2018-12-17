using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mosaic.MOL.API.DAL
{
    public class DocumentTypeDAO
    {
        private String connString;

        public DocumentTypeDAO(string connString)
        {
            this.connString = connString;
        }

        public IEnumerable<DocumentType> ListMasterTypes()
        {
            IEnumerable<DocumentType> result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                string sql = @"select distinct
                                      tm.cd_tipo_ordem Id,
                                      tm.ds_tipo_ordem Description
                                 from vnd.tipo_ordem tm
                                where tm.ic_master_contract = 'S'
                                order by tm.cd_tipo_ordem";
                result = connection.Query<DocumentType>(sql, commandType: CommandType.Text);
            }
            return result;
        }
    }
}
