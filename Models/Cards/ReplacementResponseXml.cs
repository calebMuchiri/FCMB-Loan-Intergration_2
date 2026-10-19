using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OmniApi.Models.Cards
{
    public class ReplacementResponseXml
    {
        #region SOAP Fault Envelope
        [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public class Envelope
        {
            [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public FaultBody Body { get; set; }
        }

        public class FaultBody
        {
            [XmlElement(ElementName = "Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public SoapFault Fault { get; set; }
        }

        public class SoapFault
        {
            [XmlElement("faultcode")]
            public string FaultCode { get; set; }

            [XmlElement("faultstring")]
            public string FaultString { get; set; }

            [XmlElement("detail")]
            public FaultDetail Detail { get; set; }
        }

        public class FaultDetail
        {
            [XmlElement("FaultType", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public FaultType FaultType { get; set; }
        }

        public class FaultType
        {
            [XmlElement("text", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public string Text { get; set; }
        }
        #endregion

        #region Success Application Envelope
        [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public class ApplicationEnvelope
        {
            [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public ApplicationBody Body { get; set; }
        }

        public class ApplicationBody
        {
            [XmlElement(ElementName = "application", Namespace = "http://sv.bpc.in/SVAP/iss")]
            public Application Application { get; set; }
        }

        [XmlType(Namespace = "http://sv.bpc.in/SVAP/iss")]
        public class Application
        {
            [XmlElement("application_id")]
            public string ApplicationId { get; set; }

            [XmlElement("application_type")]
            public string ApplicationType { get; set; }

            [XmlElement("application_flow_id")]
            public string ApplicationFlowId { get; set; }

            [XmlElement("customer")]
            public Customer Customer { get; set; }
        }

        public class Customer
        {
            [XmlElement("contract")]
            public Contract Contract { get; set; }
        }

        public class Contract
        {
            [XmlElement("card")]
            public Card Card { get; set; }
        }

        public class Card
        {
            [XmlElement("reissue_product_id")]
            public string ReissueProductId { get; set; }

            [XmlElement("reissue_card_id")]
            public string ReissueCardId { get; set; }

            [XmlElement("card_number")]
            public string CardNumber { get; set; }

            [XmlElement("reissue_card_number")]
            public string ReissueCardNumber { get; set; }

            [XmlElement("reissue_expiration_date")]
            public DateTime ReissueExpirationDate { get; set; }

            [XmlElement("cardholder")]
            public Cardholder Cardholder { get; set; }
        }

        public class Cardholder
        {
            [XmlElement("cardholder_name")]
            public string CardholderName { get; set; }
        }
        #endregion
    }
}
