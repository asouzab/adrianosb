using Dapper;
using Mosaic.MOL.API.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic.MOL.API.DAL
{
    public class SecurityDAO
    {
        private string connString;

        public SecurityDAO(string connString)
        {
            this.connString = connString;
        }



        public List<MenuItem> GetMenu(string appSymbol, int userId, string rootUrl)
        {
            List<MenuItem> res = new List<MenuItem>();

            using (IDbConnection connection = new OracleConnection(this.connString))
            {
                connection.Open();
                var parameters = new OracleDynamicParameters();
                parameters.Add("p_cd_usuario", value: userId, dbType: OracleDbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("p_sigla_sis", value: appSymbol, dbType: OracleDbType.Char, direction: ParameterDirection.Input);
                parameters.Add("p_result", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);

                List<MenuItem> list = new List<MenuItem>();
                MenuItem deepSearcheableMenuItem = new MenuItem
                {
                    items = list
                };
                connection.Query<MenuItem, MenuItem, MenuItem>(
                    "vnd.gx_contract_master.px_menu",
                    (item, childItem) =>
                    {
                        if(childItem.Id == 0)
                        {
                            //list.Add(item);
                            return item;
                        }

                        if(list.Count() == 0)
                        {
                            list.Add(item);
                            return item;
                        }

                        //var father = from i in list
                        //             where i.Id == childItem.Id
                        //             select i;

                        var matches = MenuItem.DepthFirstSearch(deepSearcheableMenuItem, t => t.Id == childItem.Id).ToList();

                        //var father = from i in list
                        //        from j in i.Descendants()
                        //        where j.Id == childItem.Id
                        //        select j;

                        if(matches.Count() > 0)
                        {
                            // Father exists.
                            matches.First().items.Add(item);
                        }
                        else
                        {
                            list.Add(item);
                        }

                        return item;
                    },
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                res = list;
            }

            return res;



            //try
            //    {
            //    int count = 1;
            //    int acaoBase = 0;
            //    int acaoSupAtual = 0;
            //    string url = String.Empty;

            //    conn.Open("Mosaic.ELO.Persistence.SecurityDAO.GetMenu");

            //    ProcedureParameters p = new ProcedureParameters();
            //    p.Add("P_CD_USUARIO", userId);
            //    p.Add("P_SIGLA_SIS", appSymbol);

            //    using (IDataReader rdr = conn.DbManager.ExecuteReaderProcedure("VND.GX_ELO_SECURITY.PX_MENU", p.getParametersArray(), "P_RETORNO"))
            //    {
            //        while (rdr.Read())
            //        {
            //            if (count == 1)
            //            {
            //                acaoBase = Converter.ToInt32(rdr["CD_ACAO"]);
            //                count++;
            //                continue;
            //            }

            //            url = String.Empty;

            //            if (Converter.ToInt32(rdr["CD_ACAO_SUPERIOR"]) > acaoBase)
            //            {
            //                acaoSupAtual = Converter.ToInt32(rdr["CD_ACAO_SUPERIOR"]);

            //                // É filho de alguém. Busca esse alguém na lista:
            //                Domain.MenuItem item = Busca(res, acaoSupAtual);
            //                if (item != null)
            //                {
            //                    if (item.items == null)
            //                    {
            //                        item.items = new List<Domain.MenuItem>();
            //                    }

            //                    url = Converter.ToString(rdr["DS_ACAO"]);
            //                    if (!String.IsNullOrEmpty(url))
            //                    {
            //                        url = rootUrl + Converter.ToString(rdr["DS_ACAO"]);
            //                    }

            //                    item.items.Add(new Domain.MenuItem()
            //                    {
            //                        id = Converter.ToInt32(rdr["CD_ACAO"]),
            //                        text = Converter.ToString(rdr["DS_MENU"]),
            //                        url = url
            //                    });
            //                }
            //                else
            //                {
            //                    // Não achou a ação pai.
            //                    Exception exNotFound = new Exception(string.Concat("Não foi encontrada a ação pai: ", Converter.ToString(rdr["CD_ACAO_SUPERIOR"]), " para montar o menu."));
            //                    throw exNotFound;
            //                }
            //            }
            //            else
            //            {
            //                // É órfão.
            //                if (Converter.ToInt32(rdr["CD_ACAO"]) > acaoBase)
            //                {
            //                    url = Converter.ToString(rdr["DS_ACAO"]);
            //                    if (!String.IsNullOrEmpty(url))
            //                    {
            //                        url = rootUrl + Converter.ToString(rdr["DS_ACAO"]);
            //                    }

            //                    res.Add(new Domain.MenuItem()
            //                    {
            //                        id = Converter.ToInt32(rdr["CD_ACAO"]),
            //                        text = Converter.ToString(rdr["DS_MENU"]),
            //                        url = url
            //                    });
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    conn.Close();
            //}
        }
    }
}
