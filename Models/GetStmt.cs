using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    //public class GetStmtModel
    //{
    //    public ICollection<Accounts> data { get; set; }
    //}
    public class GetStmt
    {
        public string OurBranchID { get; set; }
        public string AccountID { get; set; }
        public string MobileNumber { get; set; }
        public string FromDate { get; set; }
        public string Todate { get; set; }
        public int TopCountTrx { get; set; }
    }
}