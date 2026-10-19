using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.FCMBLoan
{
    public class AccountStatusResponse
    {
        public Data data { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
    public class Data
    {
        public string AccountStatus { get; set; }
        public string Acct_cls_flg { get; set; }
        public string Foracid { get; set; }
    }
}
