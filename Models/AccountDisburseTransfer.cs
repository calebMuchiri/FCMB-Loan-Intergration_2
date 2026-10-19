using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    public class AccountDisburseTransfer
    {
      
        public string AccountID { get; set; }
        public string MobileNumber { get; set; }
        public int ColumnID { get; set; }
        public string Narration { get; set; } = "Disbursement";
        public string UniqueRefID { get; set; } = string.Empty;
        public string OperatorID { get; set; } = "SYS";
        public string BRCheckSum { get; set; } = "No CheckSum";

    }
}