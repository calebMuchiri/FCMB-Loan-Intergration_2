using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OmniApi.Models.Cards
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class FaultEnvelope
    {
        [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public Body Body { get; set; }
    }

    public class Body
    {
        [XmlElement(ElementName = "Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public Fault Fault { get; set; }
    }

    public class Fault
    {
        [XmlElement(ElementName = "faultcode")]
        public string FaultCode { get; set; }

        [XmlElement(ElementName = "faultstring")]
        public string FaultString { get; set; }

        [XmlElement(ElementName = "detail")]
        public Detail Detail { get; set; }
    }

    public class Detail
    {
        [XmlElement(ElementName = "FaultType", Namespace = "http://sv.bpc.in/SVAP/iss")]
        public FaultType FaultType { get; set; }
    }

    public class FaultType
    {
        [XmlElement(ElementName = "text")]
        public string Text { get; set; }
    }
}
