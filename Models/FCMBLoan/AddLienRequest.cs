using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace OmniApi.Models.FCMBLoan
{
    public class AddLienRequest
    {
        public string requestID { get; set; }
        public string accountNo { get; set; }
        public decimal amountValue { get; set; }
        public string currencyCode { get; set; }
        public string rmks { get; set; }
        [DefaultValue("040")]
        public string reasonCode { get; set; } = "040"; // Default value for reasonCode
        public DateTime StartDT { get; set; }
        public DateTime EndDT { get; set; }
        public int RowID { get; set; }
    }

}
