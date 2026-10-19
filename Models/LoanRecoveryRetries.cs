using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    public class LoanRecoveryRetries
    {
        
        public string AccountID { get; set; }
        public string MobileNumber { get; set; }
        public int Trx_RowID { get; set; }
        public string LoanAccountID { get; set; } = string.Empty;
        public string ApplicationID { get; set; } = string.Empty;
        public int Retries { get; set; } = 0;
        public string ErrorDescription { get; set; } = string.Empty;
        public string OperatorID { get; set; } = "SYS";
        public string LienReferenceID { get; set; }






    }
}