using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Collections.Generic;

namespace WebApi.Jwt.Models
{
    //public class CustQueryModel
    //{
    //    public ICollection<Accounts> data { get; set; }
    //}
    public class CustQuery
    {
        public string OurBranchID { get; set; }
        public string AccountID { get; set; }
        //public string ClientID { get; set; } For Get Account Details we use ourBranchID and AccountID

    }

    public class ClientAccQuery
    {
        
        public string ClientID { get; set; } //For Getting all accounts of a client- Only taking ClientID

    }

    public class MinistatementQuery
    {
        public string OurBranchID { get; set; }
        
       public string AccountID { get; set; }
       public string TopCountTrx { get; set; } = "0";
    }

    public class AcBalQuery
    {
        
        public string OurBranchID { get; set; } = "160";
        //public string ClientID { get; set; }
        public string AccountID { get; set; } = string.Empty;
        //public string MobileNumber { get; set; } = string.Empty;

    
    }





   
}