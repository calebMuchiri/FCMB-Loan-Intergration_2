using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.FCMBLoan
{
    public class LoanTransferSummaryResponse
    {
        public string OverallStatus { get; set; } = "00"; // "00" if all succeed, "99" if any fails
        public string OverallMessage { get; set; } = "Disbursement Summary";
        public List<LoanTransferResult> Results { get; set; } = new List<LoanTransferResult>();
    }
    public class LoanTransferResult
    {
        public string CustomerReference { get; set; }
        public string StatusCode { get; set; } // "00" or "99"
        public string Message { get; set; }
    }
}
