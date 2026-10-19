using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class CardsRequestXml
    {
        public string inst_Id { get; set; } = "1002";
        public string card_mask { get; set; }
        public decimal get_balance { get; set; } = 1;
        public string lang { get; set; } = "LANGENG";
    }
}

