using System.Collections.Generic;
using System.Linq;

namespace Mosaic.MOL.API.Model
{
    public class MasterContract : Contract
    {
        public User ModifyUser { get; set; }
        public User GeneratorUser { get; set; }
        public User SenderToSAPUser { get; set; }
        public List<MasterContractItem> MasterContractItems { get; set; }
        public List<NormalContract> NormalContracts { get; set; }

        public MasterContract()
        {
            MasterContractItems = new List<MasterContractItem>();
            NormalContracts = new List<NormalContract>();
        }


        public bool CanSendToSAP
        {
            get
            {
                if (DocumentType != null &&
                    SalesOrganization != null &&
                    DistributionChannel != null &&
                    SalesDivision != null &&
                    SalesSupervisor != null &&
                    Customer != null &&
                    StartDate != null &&
                    EndDate != null &&
                    MasterContractItems.Count > 0 &&
                    IsActive)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Disabled
        {
            get
            {
                return Status > 6 || !IsActive;
            }
        }

        public bool CanReset
        {
            get
            {
                var contractsWithSAPNumber = from normalContract in NormalContracts
                                             where !string.IsNullOrEmpty(normalContract.Number)
                                             select normalContract;
                return contractsWithSAPNumber.Count() == 0;
            }
        }

        public ValidationResult Validate()
        {
            ValidationResult validationResult = new ValidationResult(isValid: true);

            if (SalesSupervisor == null)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("Informe o supervisor de vendas.");
            }

            if (Customer == null)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("Informe o cliente.");
            }

            if (DocumentType == null)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("Informe o tipo.");
            }

            if (SalesOrganization == null)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("Informe a organização de vendas.");
            }

            if (DistributionChannel == null)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("Informe o canal de distribuição.");
            }

            if (StartDate == null)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("Informe a data de início.");
            }

            if (EndDate == null)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("Informe a data fim.");
            }

            if (MasterContractItems == null || MasterContractItems.Count == 0)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("Informe ao menos um produto.");
            }

            return validationResult;
        }
    }
}
