using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using Mosaic.MOL.API.Service;
using Mosaic.MOL.API.Web.CreateContractService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Mosaic.MOL.API.Web.Controllers
{
    public class ContractsController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [HttpPost]
        public Result Save([FromBody]MasterContract masterContract)
        {
            Result result = new Result(success: true);
            ValidationResult validationResult = masterContract.Validate();
            if (!validationResult.IsValid)
            {
                result.Success = false;
                result.Messages = validationResult.Messages;
            } else
            {
                ContractService contractService = new ContractService();
                MasterContract savedMasterContract = contractService.SaveMasterContract(masterContract);
                result.Payload = savedMasterContract;
            }
            return result;
        }


        [HttpGet]
        public Result SendMasterContractToSAP(int masterContractId, int userId)
        {
            Result finalResult = new Result(success: false);
            ContractService contractService = new ContractService();
            ContractDAO contractDAO = new ContractDAO(connectionString);

            try
            {
                MasterContract masterContract = contractDAO.GetMasterContract(masterContractId);
                List<NormalContract> normalContracts = contractService.MakeContractsFromMasterItems(masterContract);

                if (normalContracts.Count > 0)
                {
                    contractDAO.SetQuantityOfContractsInMasterContract(masterContractId, normalContracts.Count);
                    List<SendToSAPResult> allResultsFromSendToSAP = new List<SendToSAPResult>();

                    foreach (NormalContract normalContract in normalContracts)
                    {
                        normalContract.CreationUser = new User { Id = userId };
                        GenerateAndSaveNormalContractAndItems(normalContract);
                        List<Result> listOfResultsForThisContract = SendNormalContractToSAP(normalContract);
                        var resultsWithFailures = from contractResult in listOfResultsForThisContract
                                                  where contractResult.Success == false
                                                  select contractResult;
                        allResultsFromSendToSAP.Add(new SendToSAPResult
                        {
                            NormalContract = normalContract,
                            Payload = listOfResultsForThisContract,
                            Success = (resultsWithFailures.Count() == 0)
                        });
                    }
                    var failedResults = from r in allResultsFromSendToSAP
                                        from r1 in r.Payload as List<Result>
                                        where r1.Success == false
                                        select r1;
                    if (failedResults.Count() == 0)
                    {
                        finalResult.Success = true;
                    }

                    // Update the Master Contract as generated and sent to SAP.
                    masterContract.GeneratorUser = new User { Id = userId };
                    masterContract.SenderToSAPUser = new User { Id = userId };
                    contractDAO.SetMasterContractSentToSAP(masterContract);

                    finalResult.Payload = allResultsFromSendToSAP;
                }
                else
                {
                    finalResult.Messages.Add("Contratos não gerados.");
                }
            }
            catch (Exception ex)
            {
                finalResult.Messages.Add(ex.Message);
            }
            return finalResult;
        }


        private void GenerateAndSaveNormalContractAndItems(NormalContract normalContract)
        {
            ContractService contractService = new ContractService();
            NormalContract savedNormalContract = contractService.GenerateAndSaveNormalContractAndItems(normalContract);
            normalContract.Id = savedNormalContract.Id;
            normalContract.PONumber = savedNormalContract.PONumber;
            normalContract.NormalContractItems = savedNormalContract.NormalContractItems;
        }


        private List<Result> SendNormalContractToSAP(NormalContract normalContract)
        {
            List<Result> listOfResults = (List<Result>)WebServiceInvocator(normalContract/*savedNormalContract*/);

            if (listOfResults.Count > 0)
            {
                var resultsWithContractNumber = from r in listOfResults
                                                where r.Success == true && r.Messages.First() == "CONTRACT_NUMBER"
                                                select r;
                if (resultsWithContractNumber.Count() > 0)
                {
                    ContractService contractService = new ContractService();
                    var sapContractNumber = resultsWithContractNumber.First().Payload.ToString();
                    normalContract.Number = sapContractNumber;
                    Result sentToSAPResult = contractService.SetNormalContractSentToSAP(normalContract/*savedNormalContract*/, sapContractNumber);
                    listOfResults.Add(sentToSAPResult);
                }
                else
                {
                    listOfResults.Add(new Result
                    (
                        success: false,
                        message: "Contrato não enviado para o SAP."
                    ));
                }
            }
            else
            {
                listOfResults.Add(new Result
                (
                    success: false,
                    message: "Não foi possível enviar o contrato para o SAP."
                ));
            }
            return listOfResults;
        }


        /// <summary>
        /// Call to ZSDI3003_CONTRACT_CREATE.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        private IEnumerable<Result> WebServiceInvocator(NormalContract contract)
        {
            List<Result> result = new List<Result>();

            // Import tab in ZSDI3003_CONTRACT_CREATE.
            BAPISDHD1 contract_header_in = new BAPISDHD1();
            BAPISDHD1X contract_header_inx = new BAPISDHD1X();
            BAPISDLS logic_switch = new BAPISDLS();

            // Tables tab in ZSDI3003_CONTRACT_CREATE.
            BAPICUBLB[] contract_cfgs_blob = new BAPICUBLB[1];
            BAPICUINS[] contract_cfgs_inst = new BAPICUINS[1];
            BAPICUPRT[] contract_cfgs_part_of = new BAPICUPRT[1];
            BAPICUCFG[] contract_cfgs_ref = new BAPICUCFG[1];
            BAPICUREF[] contract_cfgs_refinst = new BAPICUREF[1];
            BAPICUVAL[] contract_cfgs_value = new BAPICUVAL[1];
            BAPICUVK[] contract_cfgs_vk = new BAPICUVK[1];
            BAPICONDX[] contract_conditions_inx = new BAPICONDX[1];
            BAPICTR[] contract_data_in = new BAPICTR[1];
            BAPICTRX[] contract_data_inx = new BAPICTRX[1];
            BAPISDITMX[] contract_items_inx = new BAPISDITMX[1];
            BAPISDKEY[] contract_keys = new BAPISDKEY[1];
            BAPIPARNR[] contract_partners = new BAPIPARNR[2];
            BAPIPAREX[] extensionin = null;
            BAPIADDR1[] partneraddresses = new BAPIADDR1[1];
            BAPIRET2[] breturn = new BAPIRET2[1];
            BAPISDTEXT[] contract_text = null;
            BAPICOND[] contract_conditions_in = null; 
            BAPISDITM[] contract_items_in = new BAPISDITM[contract.NormalContractItems.Count];

            //// HEADER ----------------------------------------------------------------------
            contract_header_in.DOC_TYPE = contract.DocumentType.Id;
            contract_header_in.SALES_ORG = contract.SalesOrganization.Id;
            contract_header_in.DISTR_CHAN = contract.DistributionChannel.Id;
            contract_header_in.DIVISION = contract.SalesDivision.Id;
            contract_header_in.CT_VALID_F = contract.StartDate.Value.ToString("yyyy-MM-dd");
            contract_header_in.CT_VALID_T = contract.EndDate.Value.ToString("yyyy-MM-dd");
            contract_header_in.PURCH_NO_C = contract.PONumber.ToString();
            contract_header_in.SALES_GRP = contract.SalesSupervisor.Code;
            contract_header_in.SALES_OFF = contract.SalesSupervisor.Superior.Code;

            //// ITEMS ----------------------------------------------------------------------
            int itemsIndex = 0;

            foreach(NormalContractItem item in contract.NormalContractItems)
            {
                BAPISDITM bapisditm = new BAPISDITM();
                bapisditm.ITM_NUMBER = string.Format("{0,6:000000}", item.Id);
                bapisditm.MATERIAL = item.Material.Id;
                bapisditm.TARGET_QTY = item.Quantity;
                bapisditm.TARGET_QTYSpecified = true;
                bapisditm.TARGET_QU = "MTN";
                bapisditm.INCOTERMS1 = item.Incoterms.Id;
                bapisditm.INCOTERMS2 = item.Region.Name;

                contract_items_in[itemsIndex] = bapisditm;
                itemsIndex++;
            }

            // CUSTOMER ----------------------------------------------------------------------
            /*
            * AG = SP - EMISSOR DA ORDEM
            * PY = RG - PAGADOR
            * SH = WE - RECEBEDOR
            * ZW = ZW - RECEBEDOR'
            * ZA = ZA - AGENTE DE VENDAS
            */
            BAPIPARNR contract_partners_SoldTo = new BAPIPARNR();
            contract_partners_SoldTo.PARTN_ROLE = "SP";
            contract_partners_SoldTo.PARTN_NUMB = contract.Customer.Id;
            contract_partners[0] = contract_partners_SoldTo;

            //// SEND THE CONTRACT ----------------------------------------------------------------------
            NetworkCredential lNetCredentials = new NetworkCredential();
            lNetCredentials.UserName = ConfigurationManager.AppSettings["SI_CREATE_CONTRACT_OB_USER"];
            lNetCredentials.Password = ConfigurationManager.AppSettings["SI_CREATE_CONTRACT_OB_PWD"];

            SI_CREATE_CONTRACT_OB_BrazilMOService service = new SI_CREATE_CONTRACT_OB_BrazilMOService();
            service.Credentials = lNetCredentials;
            service.Timeout = int.MaxValue;

            try
            {
                string res = service.SI_CREATE_CONTRACT_OB_BrazilMO(
                    null,
                    null,
                    contract_header_in,
                    contract_header_inx,
                    "X",
                    string.Empty, //IM_ZZREPCC (NU_CONTRATO_SUBSTITUI),
                    "X",
                    logic_switch,
                    "",
                    null,
                    string.Empty, //TESTRUN
                    ref contract_cfgs_blob,
                    ref contract_cfgs_inst,
                    ref contract_cfgs_part_of,
                    ref contract_cfgs_ref,
                    ref contract_cfgs_refinst,
                    ref contract_cfgs_value,
                    ref contract_cfgs_vk,
                    ref contract_conditions_in,
                    ref contract_conditions_inx,
                    ref contract_data_in,
                    ref contract_data_inx,
                    ref contract_items_in,
                    ref contract_items_inx,
                    ref contract_keys,
                    ref contract_partners,
                    ref contract_text,
                    ref extensionin,
                    ref partneraddresses,
                    ref breturn
                );

                if (breturn != null)
                {
                    for (int i = 0; i <= breturn.Length - 1; i++)
                    {
                        if ((breturn[i].TYPE.ToUpper() == "S") && (breturn[i].NUMBER == "311"))
                        {
                            result.Add(new Result
                            (
                                success: true,
                                message: "CONTRACT_NUMBER",
                                payload: breturn[i].MESSAGE_V2
                            ));
                        }
                        else
                        {
                            result.Add(new Result
                            (
                                success: (breturn[i].TYPE == "S"),
                                message: breturn[i].MESSAGE
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add(new Result
                (
                    success: false,
                    message: ex.Message
                ));
                if (ex.InnerException != null)
                {
                    result.Add(new Result
                    (
                        success: false,
                        message: ex.InnerException.Message
                    ));
                }
            }

            return result;
        }


        [HttpGet]
        public SendToSAPResult ResendNormalContractToSAP(int normalContractId)
        {
            ContractDAO contractDAO = new ContractDAO(connectionString);
            NormalContract normalContract = contractDAO.GetNormalContractWithMaster(normalContractId);

            List<Result> results = SendNormalContractToSAP(normalContract);
            var resultsWithFailures = from contractResult in results
                                      where contractResult.Success == false
                                      select contractResult;
            SendToSAPResult sendToSAPResult = new SendToSAPResult
            {
                NormalContract = normalContract,
                Payload = results,
                Success = (resultsWithFailures.Count() == 0)
            };

            return sendToSAPResult;
        }


        [HttpGet]
        public SearchResult SearchMasterContracts(int salesUserId, int pageSize, int page)
        {
            ContractDAO contractDAO = new ContractDAO(connectionString);
            SearchResult result = contractDAO.SearchMasterContracts(salesUserId, pageSize, page);
            result.PageSize = pageSize;
            return result;
        }


        [HttpGet]
        public MasterContract GetMasterContract(int masterContractId)
        {
            ContractDAO contractDAO = new ContractDAO(connectionString);
            MasterContract masterContract = contractDAO.GetMasterContract(masterContractId);
            return masterContract;
        }


        [Route("api/contracts/latestfromcustomer")]
        [HttpGet]
        public IEnumerable<NormalContract> LastestFromCustomer(int userId, int quantity)
        {
            ContractDAO contractDao = new ContractDAO(connectionString);
            IEnumerable<NormalContract> normalContracts = contractDao.LastestFromCustomer(userId, quantity);
            return normalContracts;
        }

        [Route("api/contracts/itemsfromcontract")]
        [HttpGet]
        public IEnumerable<NormalContractItem> NormalContractItemsFromContract(int contractId)
        {
            ContractItemDAO contractItemDAO = new ContractItemDAO(connectionString);
            IEnumerable<NormalContractItem> normalContractItems = contractItemDAO.NormalContractItemsFromContract(contractId);
            return normalContractItems;
        }

        [Route("api/contracts/documentflow")]
        [HttpGet]
        public IEnumerable<NormalContractItem> DocumentFlow(int contractId)
        {
            ContractDAO contractDao = new ContractDAO(connectionString);
            IEnumerable<NormalContractItem> normalContractItems = contractDao.DocumentFlow(contractId);
            return normalContractItems;
        }

        [Route("api/contracts/searchfromcustomer")]
        [HttpGet]
        public SearchResult SearchContractsFromCustomer(int userId, string term, int pageSize, int page)
        {
            ContractDAO contractDao = new ContractDAO(connectionString);
            SearchResult searchResult = contractDao.SearchContractsFromCustomer(userId, term, pageSize, page);
            searchResult.PageSize = pageSize;
            return searchResult;
        }

        [Route("api/contracts/ResetMasterContract")]
        [HttpGet]
        public MasterContract ResetMasterContract(int masterContractId, int userId)
        {
            ContractDAO contractDao = new ContractDAO(connectionString);
            MasterContract result = contractDao.ResetMasterContract(masterContractId, userId);
            return result;
        }

        [Route("api/contracts/InactivateMasterContract")]
        [HttpGet]
        public MasterContract InactivateMasterContract(int masterContractId, int userId)
        {
            ContractDAO contractDao = new ContractDAO(connectionString);
            MasterContract result = contractDao.InactivateMasterContract(masterContractId, userId);
            return result;
        }

        [Route("api/contracts/CheckNormalContractsFromMasterContract")]
        [HttpGet]
        public IEnumerable<NormalContract> CheckNormalContractsFromMasterContract(int masterContractId)
        {
            ContractService contractService = new ContractService();
            IEnumerable<NormalContract> normalContractsNotCreated = contractService.CheckNormalContractsFromMasterContract(masterContractId);
            return normalContractsNotCreated;
        }
    }
}
