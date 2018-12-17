using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mosaic.MOL.API.DAL
{
    public class UserDAO
    {
        private String connString;

        public UserDAO(string connString)
        {
            this.connString = connString;
        }

        public IEnumerable<User> ListByType(int type)
        {
            IEnumerable<User> result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                string sql = String.Format(@"select us.cd_usuario Id,
                                                    us.no_usuario Name,
                                                    us.cd_usuario_original Code,
                                                    us_sup.cd_usuario Id,
                                                    us_sup.no_usuario Name,
                                                    us_sup.cd_usuario_original Code
                                              from ctf.usuario us
                                              left join ctf.usuario us_sup
                                                on us_sup.cd_usuario = us.cd_usuario_superior
                                             where us.cd_tipo_usuario = {0}
                                               and us.ic_ativo = 'S'
                                             order by us.no_usuario", type.ToString());
                result = connection.Query<User, User, User>(
                    sql, 
                    (user, superior) =>
                    {
                        user.Superior = superior;
                        return user;
                    },
                    commandType: CommandType.Text
                );
            }
            return result;
        }

        public IEnumerable<User> QueryByType(int type, string query)
        {
            IEnumerable<User> result;
            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                string sql = String.Format(@"select us.cd_usuario Id,
                                                         us.no_usuario Name,
                                                         us.cd_usuario_original Code,
                                                         us_sup.cd_usuario Id,
                                                         us_sup.no_usuario Name,
                                                         us_sup.cd_usuario_original Code
                                                   from ctf.usuario us
                                                   left join ctf.usuario us_sup
                                                     on us_sup.cd_usuario = us.cd_usuario_superior
                                                  where us.cd_tipo_usuario = {0}
                                                    and (
                                                               lower(us.no_usuario) like '%{1}%'
                                                            or us.cd_usuario_original like '%{1}%'
                                                        )
                                                    and us.ic_ativo = 'S'
                                                  order by us.no_usuario", type.ToString(), query.ToLower());
                result = connection.Query<User, User, User>(
                    sql,
                    (user, superior) =>
                    {
                        user.Superior = superior;
                        return user;
                    },
                    commandType: CommandType.Text
                );
            }
            return result;
        }
    }
}
