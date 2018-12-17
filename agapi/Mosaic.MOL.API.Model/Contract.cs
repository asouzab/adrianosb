using System;

namespace Mosaic.MOL.API.Model
{
    public abstract class Contract
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Active { get; set; }
        public int Status { get; set; }
        public Customer Customer { get; set; }
        public DocumentType DocumentType { get; set; }
        public SalesOrganization SalesOrganization { get; set; }
        public DistributionChannel DistributionChannel { get; set; }
        public SalesDivision SalesDivision { get; set; }
        public User SalesSupervisor { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? GeneratedDate { get; set; }
        public User CreationUser { get; set; }

        public bool IsActive
        {
            get
            {
                return String.IsNullOrEmpty(Active) ? false : (Active == "S");
            }
        }

        public bool CanInactivate
        {
            get
            {
                return this.IsActive && this.Status < 2;
            }
        }

        public string StartDate_S
        {
            get
            {
                string result = String.Empty;
                if(StartDate != null)
                {
                    result = StartDate.Value.ToString("dd/MM/yyyy");
                }
                return result;
            }
        }

        public string EndDate_S
        {
            get
            {
                string result = String.Empty;
                if(EndDate != null)
                {
                    result = EndDate.Value.ToString("dd/MM/yyyy");
                }
                return result;
            }
        }
    }
}
