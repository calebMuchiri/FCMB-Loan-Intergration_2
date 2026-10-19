using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    public class AccountCRBDetails
    {

        public string AccountID { get; set; }
        public string MobileNumber { get; set; }
        public string ApplicationID { get; set; }
        public string ClientID { get; set; }
        public string CRB_Reference { get; set; } = string.Empty;
        public string CRB_Response_Status { get; set; } = string.Empty;
        public string CRB_Description { get; set; } = string.Empty;

        public string OperatorID { get; set; } = "SYS";

        





    }
}