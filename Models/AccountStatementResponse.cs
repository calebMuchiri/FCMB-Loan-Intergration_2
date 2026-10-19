using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    
    public class AccountStatementResponse
    {
        public int ColumnID { get; set; }
        public string  TrxDate { get; set; }
        public string ValueDate { get; set; }
        public string Particulars { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Closing { get; set; }
        public decimal Amount { get; set; }
        public string TrxTypeID { get; set; }
        public string TrxCurrencyID { get; set; }
        //public string OperatorID { get; set; }
        //public string SupervisorID { get; set; }


    }
}