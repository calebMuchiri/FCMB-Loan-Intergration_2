using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{
    //public class  AccountOpeningModel    {
    //    public ICollection<Accounts> data { get; set; }
    //}
    public class ClientOpening
    {
        public string OurBranchID { get; set; }
        public string ClientType { get; set; }
        public string TitleID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Mobile { get; set; }
        public string ProductID { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string CityID { get; set; } = "Kampala";
        public string CountryID { get; set; } = "Uganda";
        public string EmailID { get; set; }
        public string ID1 { get; set; }
        public string ID2 { get; set; }
        public string GenderID { get; set; } = null;
        public string MaritalStatusID { get; set; }
        public string DateOfBirth { get; set; } = string.Empty;
        public string PassportNo { get; set; }
        public DateTime OpenedDate { get; set; } = DateTime.Now;
        public string Fax { get; set; }
        public string NationalityID { get; set; } = "UG";
        public string IdentificationTypeID { get; set; } = "FCS";
        public string ContactPerson { get; set; }
        public string ContactPersonID { get; set; }
        public string ContactPersonMobile { get; set; }
        public string Photo { get; set; }
        public string IdFront { get; set; }
        public string IdBack { get; set; }
        public string Signature { get; set; }
        public string OperatingModeID { get; set; }
        public string OperatingInstructions { get; set; }
        public string AccountClassID { get; set; }
        public string AccountOfficerID { get; set; }
        public string Comments { get; set; }
        public string OpenedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string SupervisedBy { get; set; }
        public string LegacyAccountID { get; set; } = null;
        public string LegacyClientID { get; set; } = null;
        public string FCS_Number { get; set; } = null;
        public string EmployerName { get; set; }
        /// public DateTime WorkingSince { get; set; } = default(DateTime);
        public decimal Salary { get; set; } = 0;
        public int NewRecord { get; set; } = 1;
        public bool UpdateMobileNumber { get; set; } = false;
        public bool UpdateLegacyAccountID { get; set; } = false;
        public string Occupation { get; set; } 


        //other details

        public string ClientTypeID { get; set; }
        public string Age { get; set; }
        public string NextOfKinID { get; set; }
        public string Email { get; set; }

        //group details section
        public string GroupID { get; set; }
        public string SubGroupID { get; set; }
        public string JoinDate { get; set; }

    }
}