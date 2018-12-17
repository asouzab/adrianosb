using System;
using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class NotaFiscal
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public string Series { get; set; }
        public string DanfeKey { get; set; }
        public DateTime? IssueDate { get; set; }
        public List<NotaFiscalItem> NotaFiscalItems { get; set; }

        public NotaFiscal()
        {
            NotaFiscalItems = new List<NotaFiscalItem>();
        }

        public string IssueDate_S
        {
            get
            {
                return IssueDate == null ? "" : IssueDate.Value.ToString("dd/MM/yyyy");
            }
        }
    }
}
