using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.FCMBLoan
{
    public class AddLienResult
    {
        public string requestId { get; set; }
        public string accountNo { get; set; }
        public bool success { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public AddLienData data { get; set; }
    }

    public class AddLienBatchResponse
    {
        public int requested { get; set; }
        public int succeeded { get; set; }
        public int failed { get; set; }
        public List<AddLienResult> results { get; set; } = new List<AddLienResult>();
    }
}
