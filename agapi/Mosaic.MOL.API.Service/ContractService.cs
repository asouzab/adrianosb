using Mosaic.MOL.API.DAL;
using Mosaic.MOL.API.Model;
using Mosaic.MOL.API.Utils;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Mosaic.MOL.API.Service
{
    public class ContractService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string connectionString = ConfigurationManager.ConnectionStrings["MOL"].ConnectionString;

        public MasterContract SaveMasterContract(MasterContract masterContract)
        {
            ContractDAO contractDao = new ContractDAO(connectionString);
            MasterContract result;
            if (masterContract.Id == 0)
            {
                result = contractDao.InsertMasterContract(masterContract);
            }
            else
            {
                result = contractDao.UpdateMasterContract(masterContract);
            }

            if (result != null)
            {
                masterContract.Id = result.Id;
                masterContract.Active = result.Active;
                SaveContractMasterItems(masterContract);
                SavePeriods(masterContract);
            }

            return masterContract;
        }


        private void SaveContractMasterItems(MasterContract masterContract)
        {
            ContractItemDAO contractItemDAO = new ContractItemDAO(connectionString);
            MasterContractItem result;
            foreach (MasterContractItem contractMasterItem in masterContract.MasterContractItems)
            {
                if (contractMasterItem.Id == 0)
                {
                    contractMasterItem.CreationUser = masterContract.CreationUser;
                    result = contractItemDAO.InsertContractMasterItem(masterContract.Id, contractMasterItem);
                }
                else
                {
                    contractMasterItem.ModifyUser = masterContract.ModifyUser;
                    contractMasterItem.ContractId = masterContract.Id;
                    result = contractItemDAO.UpdateContractMasterItem(contractMasterItem);
                }
                if (result != null)
                {
                    contractMasterItem.Id = result.Id;
                }
            }
        }

        private void SavePeriods(MasterContract masterContract)
        {
            PeriodDAO periodDAO = new PeriodDAO(connectionString);
            Period result;

            // Remove existing periods in database that didn't come in the post.
            List<Period> periodsFromDB = (List<Period>)periodDAO.ListByContract(masterContract.Id);
            foreach (Period periodFromDB in periodsFromDB)
            {
                var x = from mi in masterContract.MasterContractItems
                        from p in mi.Periods
                        where p.UID == periodFromDB.UID
                        select p;
                if (x.Count() == 0)
                {
                    periodFromDB.ModifyUser = masterContract.ModifyUser;
                    periodDAO.Delete(periodFromDB);
                }
            }

            // Save the periods that came in the post.
            foreach (MasterContractItem contractMasterItem in masterContract.MasterContractItems)
            {
                foreach (Period period in contractMasterItem.Periods)
                {
                    result = period;
                    if (period.Id == 0 && period.Quantity > 0)
                    {
                        period.CreationUser = masterContract.CreationUser;
                        result = periodDAO.InsertPeriod(masterContract.Id, contractMasterItem.Id, period);
                    }
                    else if (period.Id != 0 && period.Quantity > 0)
                    {
                        period.ModifyUser = masterContract.ModifyUser;
                        result = periodDAO.UpdatePeriod(period);
                    }
                    // We want to keep the period, assuming that the start and end dates have not changed.
                    //else if (period.Id != 0 && period.Quantity == 0)
                    //{
                    //    period.ModifyUser = masterContract.ModifyUser;
                    //    periodDAO.Delete(period);
                    //}
                    if (result != null)
                    {
                        period.Id = result.Id;
                        period.Date = result.Date;
                    }
                }
            }
        }


        public List<NormalContract> MakeContractsFromMasterItems(MasterContract masterContract)
        {
            // For each period of every contract master item, 
            // create a new contract and add it to a list.
            //
            // Example:
            //
            // master item #    | Period 1 | Period 2 | Period 3 |
            // -----------------|----------|----------|----------|
            // 10               |     90   |    150   |    230   |
            // 20               |    120   |     70   |      0   |
            //
            // Would create 5 contracts with one item each.
            List<NormalContract> contractsFromPeriods = new List<NormalContract>();
            foreach (MasterContractItem masterItem in masterContract.MasterContractItems)
            {
                var periodsWithQuantity = masterItem.Periods.Where(p => p.Quantity > 0);
                foreach (Period period in periodsWithQuantity)
                {
                    NormalContract contract = new NormalContract()
                    {
                        StartDate = period.Date,
                        DocumentType = masterContract.DocumentType,
                        DistributionChannel = masterContract.DistributionChannel,
                        SalesOrganization = masterContract.SalesOrganization,
                        SalesDivision = masterContract.SalesDivision,
                        SalesSupervisor = masterContract.SalesSupervisor,
                        Customer = masterContract.Customer,
                        MasterContractId = masterContract.Id
                    };
                    contract.NormalContractItems.Add(new NormalContractItem
                    {
                        Material = masterItem.Material,
                        Incoterms = masterItem.Incoterms,
                        Region = masterItem.Region,
                        Quantity = period.Quantity
                    });
                    contractsFromPeriods.Add(contract);
                }
            }

            // Group the contracts by Start Date.
            var contractsGroupped = contractsFromPeriods
                .OrderBy(p => p.StartDate)
                .GroupBy(p => p.StartDate)
                .Select(grp => grp.ToList())
                .ToList();

            // For every group of contracts, create a new Contract with the items merged.
            //
            // Example:
            //
            // master item #    | Period 1 | Period 2 | Period 3 |
            // -----------------|----------|----------|----------|
            // 10               |     90   |    150   |    230   |
            // 20               |    120   |     70   |      0   |
            //
            // Would create 3 contracts:
            // - Contract 1: Two items => 90t and 120t.
            // - Contract 2: Two items => 150t and 70t.
            // - Contract 3: One item => 230t.
            var contracts = from g in contractsGroupped
                            select new NormalContract
                            {
                                StartDate = g.First().StartDate,
                                EndDate = g.First().StartDate.Value.LastDayOfMonth(),
                                Customer = g.First().Customer,
                                DistributionChannel = g.First().DistributionChannel,
                                DocumentType = g.First().DocumentType,
                                SalesOrganization = g.First().SalesOrganization,
                                SalesDivision = g.First().SalesDivision,
                                SalesSupervisor = g.First().SalesSupervisor,
                                MasterContractId = g.First().MasterContractId,
                                NormalContractItems = (
                                                    from c in g
                                                    from i in c.NormalContractItems
                                                    select i
                                                ).ToList()
                            };

            List<NormalContract> finalListOfContracts = contracts.OrderBy(c => c.StartDate).ToList();

            finalListOfContracts.First().StartDate = masterContract.StartDate;
            finalListOfContracts.Last().EndDate = masterContract.EndDate;

            return finalListOfContracts;
        }


        /// <summary>
        /// Generates the PO Number and creates the cadence data in 
        /// MOL database to avoid dump in ZSDI3003_CONTRACT_CREATE.
        /// </summary>
        /// <param name="contract">The contract generated from Master Contract Items.</param>
        public NormalContract GenerateAndSaveNormalContractAndItems(NormalContract normalContract)
        {
            ContractDAO contractDAO = new ContractDAO(connectionString);
            NormalContract savedNormalContract = contractDAO.GenerateNormalContract(normalContract);

            foreach (NormalContractItem normalContractItem in normalContract.NormalContractItems)
            {
                normalContractItem.ContractId = savedNormalContract.Id;
                NormalContractItem savedNormalContractItem = contractDAO.InsertNormalContractItem(normalContractItem, normalContract.StartDate.Value, normalContract.CreationUser);
                savedNormalContractItem.Incoterms = normalContractItem.Incoterms;
                savedNormalContractItem.Region = normalContractItem.Region;
                savedNormalContract.NormalContractItems.Add(savedNormalContractItem);
            }

            return savedNormalContract;
        }

        public Result SetNormalContractSentToSAP(NormalContract normalContract, string SAPNumber)
        {
            ContractDAO contractDAO = new ContractDAO(connectionString);
            SAPNumber = SAPNumber.PadLeft(10, '0');
            Result result = contractDAO.SetNormalContractSentToSAP(normalContract, SAPNumber);
            return result;
        }

        public List<NormalContract> CheckNormalContractsFromMasterContract(int masterContractId)
        {
            List<NormalContract> contractsNotFound = new List<NormalContract>();
            ContractDAO contractDAO = new ContractDAO(connectionString);
            MasterContract masterContract = contractDAO.GetMasterContract(masterContractId);
            this.MakeContractsFromMasterItems(masterContract);

            List<NormalContract> normalContractsThatShouldExist = this.MakeContractsFromMasterItems(masterContract);

            foreach(NormalContract normalContractThatShouldExist in normalContractsThatShouldExist)
            {
                IEnumerable<NormalContract> contractsFound = from existingNormalContract in masterContract.NormalContracts
                                                             where existingNormalContract.StartDate == normalContractThatShouldExist.StartDate && existingNormalContract.EndDate == normalContractThatShouldExist.EndDate
                                                             select existingNormalContract;
                        
                if (contractsFound.Count() == 0)
                {
                    contractsNotFound.Add(normalContractThatShouldExist);
                    continue;
                }
                else
                {
                    foreach(NormalContractItem itemThatShouldExist in normalContractThatShouldExist.NormalContractItems)
                    {
                        IEnumerable<NormalContractItem> itemsFound = from existingNormalContract in contractsFound
                                                            from existingItem in existingNormalContract.NormalContractItems
                                                            where existingItem.Material.Id == itemThatShouldExist.Material.Id && existingItem.Quantity == itemThatShouldExist.Quantity
                                                            select existingItem;
                        if(itemsFound.Count() == 0)
                        {
                            contractsNotFound.Add(normalContractThatShouldExist);
                            continue;
                        }
                    }
                }
            }
            return contractsNotFound;
        }
    }
}
