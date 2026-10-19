using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    //public class LoansQueryModel
    //{
    //    public ICollection<Accounts> data { get; set; }
    //}
    public class LoansQuery
    {
        public string ClientID { get; set; }
        public string OurBranchID { get; set; }
        public string AccountID { get; set; }       
    }
}