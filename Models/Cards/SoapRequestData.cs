using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class SoapRequestData
    {
        public string ApplicationNumber { get; set; }
        public string ApplicationDate { get; set; }
        public string ApplicationType { get; set; }
        public string ApplicationFlowId { get; set; }
        public string ApplicationStatus { get; set; }
        public string OperatorId { get; set; }
        public string InstitutionId { get; set; }
        public string AgentNumber { get; set; }
        public string CustomerType { get; set; }
        public CustomerData Customer { get; set; }
    }
    public class CustomerData
    {
        public string Command { get; set; }
        public string CustomerNumber { get; set; }
        public ContractData Contract { get; set; }
        public PersonData Person { get; set; }
        public ContactData Contact { get; set; }
        public AddressData Address { get; set; }
    }


    public class ContractData
    {
        public string Command { get; set; }
        public string ContractType { get; set; }
        public string ProductNumber { get; set; }
        public CardData Card { get; set; }
        public ServiceData[] Service { get; set; }
        public AccountData Account { get; set; }
    }

    public class CardData
    {
        public string Command { get; set; }
        public string CardType { get; set; }
        public CardholderData Cardholder { get; set; }
    }
    public class CardholderData
    {
        public string Command { get; set; }
        public string CardholderName { get; set; }
        public NotificationData Notification { get; set; }
    }
    public class ServiceData
    {
        public string Value { get; set; }
    }

    public class NotificationData
    {
        public string Command { get; set; }
        public string DeliveryChannel { get; set; }
        public string ContactType { get; set; }
        public string DeliveryAddress { get; set; }
        public string IsActive { get; set; }
    }
    public class AccountData
    {
        public string Command { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public string AccountType { get; set; }
        public string AccountStatus { get; set; }
        public AccountObjectData AccountObject { get; set; }
    }

    public class AccountObjectData
    {
        public string AccountLinkFlag { get; set; }
        public string IsPosDefault { get; set; }
        public string IsAtmDefault { get; set; }
    }

    public class PersonData
    {
        public string Command { get; set; }
        public PersonNameData PersonName { get; set; }
        public IdentityCardData IdentityCard { get; set; }
    }

    public class PersonNameData
    {
        public string Surname { get; set; }
        public string FirstName { get; set; }
    }

    public class IdentityCardData
    {
        public string Command { get; set; }
        public string IdType { get; set; }
        public string IdSeries { get; set; }
        public string IdNumber { get; set; }
    }

    public class ContactData
    {
        public string Command { get; set; }
        public string ContactType { get; set; }
        public string PreferredLang { get; set; }
    }

    public class AddressData
    {
        public string Command { get; set; }
        public string AddressType { get; set; }
        public string Country { get; set; }
        public AddressNameData AddressName { get; set; }
        public string House { get; set; }
        public string Apartment { get; set; }
    }

    public class AddressNameData
    {
        public string Region { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
    }
}
