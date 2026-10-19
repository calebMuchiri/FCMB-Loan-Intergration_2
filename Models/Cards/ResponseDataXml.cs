using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OmniApi.Models.Cards
{
    public class ResponseDataXml
    {
        [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public class Envelope
        {
            [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public Body Body { get; set; }
        }

        public class Body
        {
            [XmlElement(ElementName = "application", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public Application Application { get; set; }

            public Fault Fault { get; set; }
        }

        public class Fault
        {
            [XmlElement(ElementName = "faultcode", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public string FaultCode { get; set; }

            [XmlElement(ElementName = "faultstring", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public string FaultString { get; set; }

            [XmlElement(ElementName = "detail", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public Detail Detail { get; set; }
        }

        public class Detail
        {
            [XmlElement(ElementName = "FaultType", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public FaultType FaultType { get; set; }
        }

        public class FaultType
        {
            [XmlElement(ElementName = "text", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Text { get; set; }
        }

        public class Application
        {
            [XmlElement(ElementName = "application_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ApplicationNumber { get; set; }

            [XmlElement(ElementName = "application_date", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ApplicationDate { get; set; }

            [XmlElement(ElementName = "application_type", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ApplicationType { get; set; }

            [XmlElement(ElementName = "application_flow_id", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ApplicationFlowId { get; set; }

            [XmlElement(ElementName = "application_status", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ApplicationStatus { get; set; }

            [XmlElement(ElementName = "operator_id", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string OperatorId { get; set; }

            [XmlElement(ElementName = "institution_id", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string InstitutionId { get; set; }

            [XmlElement(ElementName = "agent_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string AgentNumber { get; set; }

            [XmlElement(ElementName = "customer_type", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string CustomerType { get; set; }

            [XmlElement(ElementName = "customer", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public CustomerData Customer { get; set; }
        }

        public class CustomerData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "command", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Command { get; set; }

            [XmlElement(ElementName = "customer_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string CustomerNumber { get; set; }

            [XmlElement(ElementName = "contract", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public ContractData Contract { get; set; }

            [XmlElement(ElementName = "person", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public PersonData Person { get; set; }

            [XmlElement(ElementName = "contact", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public ContactData Contact { get; set; }

            [XmlElement(ElementName = "address", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public AddressData Address { get; set; }
        }

        public class ContractData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "command", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Command { get; set; }

            [XmlElement(ElementName = "contract_type", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ContractType { get; set; }

            [XmlElement(ElementName = "product_id", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ProductID { get; set; }

            [XmlElement(ElementName = "product_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ProductNumber { get; set; }

            [XmlElement(ElementName = "contract_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ContractNumber { get; set; }

            [XmlElement(ElementName = "card", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public CardData Card { get; set; }

            [XmlElement(ElementName = "service", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public ServiceData[] Service { get; set; }

            [XmlElement(ElementName = "account", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public AccountData Account { get; set; }
        }

        public class CardData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "command", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Command { get; set; }

            [XmlElement(ElementName = "card_type", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string CardType { get; set; }

            [XmlElement(ElementName = "card_id", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string CardID { get; set; }

            [XmlElement(ElementName = "card_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string CardNumber { get; set; }

            [XmlElement(ElementName = "expiration_date", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public DateTime ExpirationDate { get; set; }

            [XmlElement(ElementName = "cardholder", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public CardholderData Cardholder { get; set; }
        }

        public class CardholderData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "command", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Command { get; set; }

            [XmlElement(ElementName = "cardholder_name", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string CardholderName { get; set; }

            [XmlElement(ElementName = "cardholder_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string CardholderNumber { get; set; }

            [XmlElement(ElementName = "notification", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public NotificationData Notification { get; set; }
        }

        public class NotificationData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "command", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Command { get; set; }

            [XmlElement(ElementName = "delivery_channel", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string DeliveryChannel { get; set; }

            [XmlElement(ElementName = "contact_type", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ContactType { get; set; }

            [XmlElement(ElementName = "delivery_address", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string DeliveryAddress { get; set; }

            [XmlElement(ElementName = "is_active", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string IsActive { get; set; }
        }

        public class ServiceData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlAttribute(AttributeName = "value")]
            public string Value { get; set; }

            // You can add additional properties if needed for <service_object> elements.
        }

        public class AccountData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "command", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Command { get; set; }

            [XmlElement(ElementName = "account_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string AccountNumber { get; set; }

            [XmlElement(ElementName = "currency", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Currency { get; set; }

            [XmlElement(ElementName = "account_type", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string AccountType { get; set; }

            [XmlElement(ElementName = "account_status", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string AccountStatus { get; set; }

            [XmlElement(ElementName = "account_object", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public AccountObjectData[] AccountObject { get; set; }
        }

        public class AccountObjectData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "object_type", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ObjectType { get; set; }

            [XmlElement(ElementName = "object_value", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ObjectValue { get; set; }
        }

        public class PersonData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "last_name", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string LastName { get; set; }

            [XmlElement(ElementName = "first_name", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string FirstName { get; set; }

            [XmlElement(ElementName = "middle_name", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string MiddleName { get; set; }

            [XmlElement(ElementName = "date_of_birth", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public DateTime DateOfBirth { get; set; }

            [XmlElement(ElementName = "gender", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Gender { get; set; }

            [XmlElement(ElementName = "nationality", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Nationality { get; set; }

            [XmlElement(ElementName = "document", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public DocumentData[] Document { get; set; }
        }

        public class DocumentData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "document_type", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string DocumentType { get; set; }

            [XmlElement(ElementName = "document_number", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string DocumentNumber { get; set; }

            [XmlElement(ElementName = "expiration_date", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public DateTime ExpirationDate { get; set; }
        }

        public class ContactData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "email", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Email { get; set; }

            [XmlElement(ElementName = "phone", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Phone { get; set; }
        }

        public class AddressData
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "street", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Street { get; set; }

            [XmlElement(ElementName = "city", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string City { get; set; }

            [XmlElement(ElementName = "zip_code", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string ZipCode { get; set; }

            [XmlElement(ElementName = "country", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Country { get; set; }
        }
    }
}
