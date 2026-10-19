using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OmniApi.Models
{
    public class GroupDetailsResponse
    {
        public string OurBranchID { get; set; }
        public string GroupID { get; set; }
        public string GroupName { get; set; }

        public string GroupProductID { get; set; }
        public string DefaultLoanSchemeID { get; set; }
        public string FormationDate { get; set; }
        public string Officer1 { get; set; }
        public string GroupFormedBy { get; set; }
        public string CityID { get; set; }
        public string CenterID { get; set; }
        public string VillageID { get; set; }
        public string MeetingPlace { get; set; }
        public string MeetingFrequencyID { get; set; }
        public string MeetingDayID { get; set; }
        public string FirstMeetingDate { get; set; }
        public string GroupClassID { get; set; }
        public string MeetingTime { get; set; }
        public string NextMeetingDate { get; set; }
        public string RegistrationNo { get; set; }
        public string GroupLead1  { get; set; }
        public string GroupStatusID { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }

        public string TotalMembers { get; set; }

       public string TotalSavingACs { get; set; }
      public string TotalZeroSavingACs { get; set; }  

    public string TotalSavingsBalance { get; set; }
             public string TotalLoanACs { get; set; }

          public string TotalZeroLoanACs { get; set; }
           public string TotalLoanBalance { get; set; }

        public string LoanCycleTypeID { get; set; }
        public string LoanCycleType { get; set; }

        public string LoanCycleNo { get; set; }
   
    }
}