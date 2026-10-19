using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    //public class AccountOpeningResponse
    //{
    //    public string OurBranchID { get; set; }
    //    public string ClientID { get; set; }
    //    public string AccountID { get; set; }
    //    public string Name { get; set; }

    //}


    
    public class AccountOpeningResponse
    {
        public string Status { get; set; }
        public string Response { get; set; }
        public string Data { get; set; }
        //public string Name { get; set; }

    }


    //public class AccountOpeningExtras
    //{
    //    public string KRA_PIN_Number { get; set; }
    //    public string PhysicalLocation { get; set; }
    //    public string NextOfKin { get; set; }
    //    public string NextOfKin_ID { get; set; }
    //    public string NextOfKin_Mobile { get; set; }

    //}
}