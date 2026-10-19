using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    public class LoansModel
    {
        public ICollection<Loans> data { get; set; }
    }
    public class Loans
    {
        //public string SessionID { get; set; }
        public string OurBranchID { get; set; }
        public string ApplicationDate { get; set; }
        public string ClientID { get; set; }
        public string ProductID { get; set; }
        public string PurposeCodeID { get; set; }
        public string CreditOfficerID { get; set; }
        public string LoanAmount { get; set; }
        public string LoanTerm { get; set; }
        public string DisbursementDate { get; set; }
        public string Stage { get; set; }
        public bool IsExistingClient { get; set; }
        public string RepaymentAccountID { get; set; }
        public string LoanPeriodID { get; set; }
        public string BusinessLineID { get; set; }
        public string AccountClassID { get; set; }
        public int InterestRate { get; set; }
        public string ModifiedBy { get; set; }
        public string ApplicationID { get; set; }
    }
}