using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OmniApi.Models
{
    //public class ClientOpeningResponse
    //{
    //        public string OurBranchID { get; set; }
    //        public string ClientID { get; set; }
    //        public string AccountID { get; set; }
    //        public string Name { get; set; }
    //}  

    public class ClientOpeningResponse
    {
        public string Status { get; set; }
        public string Response { get; set; }
        public string Data { get; set; }
        //public string Name { get; set; }

    }

}