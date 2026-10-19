using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OmniApi.Models
{
    public class CollateralResponse
    {
        public string CollateralID { get; set; }
        public string Description { get; set; }
        public string OurBranchID { get; set; }
        public string CollateralTypeID { get; set; }
        public string OwnerClientID { get; set; }
        public string LodgedDate { get; set; }
        public string NatureOfChargeID { get; set; }
        public string IsInsured { get; set; }
        public string Remarks { get; set; }
        public string amount { get; set; }
        public string cost { get; set; }
        public string marketvalue { get; set; }
        public string forcedsalevalue { get; set; }


    }
}