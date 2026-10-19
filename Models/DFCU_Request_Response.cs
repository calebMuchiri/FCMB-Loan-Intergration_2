using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OmniApi.Models
{
    public class DFCU_Request_Response
    {
        
        public string MethodName { get; set; }
        public string Action { get; set; }
        public string strAddEditAPIAuditLogXML { get; set; }
        public string AuditID { get; set; }
    }
}