using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Jwt.Models
{

    public class ClientDetailResponse
    {
        public string OurBranchID { get; set; }
        public string BranchName { get; set; }
        public string ClientID { get; set; }
        public string ClientName { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string CurrencyID { get; set; }
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string CountryName { get; set; }
        public string AccountBalance { get; set; }
        public string Mobile { get; set; }
        public string EmailID { get; set; }
        public string ACStatus { get; set; }
        //public string DFCU_AccountNumber { get; set; }
        //public string DFCU_ClientNumber { get; set; }

        public string TitleID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string GenderID { get; set; }
        public string NationalityID { get; set; }
        public bool IsDOBGiven { get; set; }
        public string DateOfBirth { get; set; }
        public string Age { get; set; }
        public string AgeAsOn { get; set; }
        public string BloodGroupID { get; set; }
        public bool CanDonateBlood { get; set; }
        public string ResidentID { get; set; }
        public string LiteracyLevelID { get; set; }
        public string PassportNo { get; set; }
        public string PassportIssuedCityID { get; set; }
        public string PassportExpiryDate { get; set; }
        public string MaritalStatusID { get; set; }
        public string SpouseID { get; set; }
        public string NextOfKinID { get; set; }
        public string NumberOfHouseMembers { get; set; }
        public string NumberOfChildren { get; set; }
        public string NumberOfDependents { get; set; }
        public bool IsSalaried { get; set; }
        public string OccupationID { get; set; }
        public string DesignationID { get; set; }
        public string CompanyTypeID { get; set; }
        public string EmployerName { get; set; }
        public string WorkingSince { get; set; }
        public string Salary { get; set; }
        public string FamilyIncome { get; set; }
        public string OtherIncome { get; set; }
        public string RentExpense { get; set; }
        public string OtherExpenses { get; set; }
        public string WorkPermitNo { get; set; }
    }
}
