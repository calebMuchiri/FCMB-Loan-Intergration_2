using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class CardStatusInquiry
    {
        public class RequestData
        {
            public string Cardnumber { get; set; }
            public int MemNo { get; set; }
        }

        public class ResponseData
        {
            public int Status { get; set; }
            public decimal AvailableBalance { get; set; }
            public decimal AvailableCashBalance { get; set; }
            public decimal OutstandingBalance { get; set; }
            public int CardStatus { get; set; }
            public decimal CurPaymentAmount { get; set; }
            public int PinCount { get; set; }
            public CardLimits Limits { get; set; }
        }

        public class CardLimits
        {
            public decimal CashLimit { get; set; }
            public decimal PurchaseLimit { get; set; }
            public decimal TransferLimit { get; set; }
            public decimal AvalilableCashLimit { get; set; }
        }
    }
}
