using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.FCMBLoan
{
    public class BaseRetailAccountOpeningRequest
    {
        public string requestId { get; set; }
        public string mobileNo { get; set; }
        public string nin { get; set; }
        public bool isWalletAccount { get; set; }
        public string brokerCode { get; set; }
        public string branchCode { get; set; }
        public string accountOfficerCode { get; set; }
        public ConfigCredential configCredentials { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string motherMaidenName { get; set; }
        public DateTime dob { get; set; }
        public string gender { get; set; }
        public string email { get; set; }
        public Address address { get; set; }
        public string bvn { get; set; }
        public string maritalStatus { get; set; }
        public string occupation { get; set; }
        public string employmentStatus { get; set; }
        public string salutation { get; set; }
        public string image { get; set; }
    }

    public class ConfigCredential
    {
        public string clientId { get; set; }
        public string productId { get; set; }
    }

    public class Address
    {
        public string addrLine1 { get; set; }
        public string houseNum { get; set; }
        public string streetName { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
    }

}
