using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.FCMBLoan
{
    public class DisbursementTransferRequest
    {
        public string DebitAccountNo { get; set; }
        public string CreditAccountNo { get; set; }
        public bool IsFees { get; set; }
        public List<Charges> Charges { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string Narration { get; set; }
        public string Remark { get; set; }
        public string CustomerReference { get; set; }
        public Location location { get; set; }
    }

    public class Charges
    {
        public string FeeNarration { get; set; }
        public string Account { get; set; }
        public double Fee { get; set; }
    }

    public class Location
    {
        public int Longitude { get; set; }
        public int Latitude { get; set; }
    }
}
