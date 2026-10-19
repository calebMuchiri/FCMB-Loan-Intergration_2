using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    
    public class AccountsDetailResponse
    {
        public string OurBranchID { get; set; }
        public string BranchName { get; set; }
        public string ClientID { get; set; }
        public string ClientName { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string CurrencyID { get; set; }
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string CountryName { get; set; }
        public string AccountBalance { get; set; }
        public string Mobile { get; set; }
        public string EmailID { get; set; }
        public string ACStatus { get; set; }
        //public string DFCU_AccountNumber { get; set; }
        //public string DFCU_ClientNumber { get; set; }
    }
}