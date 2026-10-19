using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OmniApi.Models.Cards
{
    public class GetCardsResponseXMl
    {
        [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public class Envelope
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
            public Envelope()
            {
                xmlns.Add("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            }

            [XmlElement(ElementName = "Body")]
            public Body Body { get; set; }

        }

        public class Body
        {
            [XmlElement(ElementName = "getCardsResponse", Namespace = "http://bpc.ru/SVXP/integration")]
            public GetCardsResponse GetCardsResponse { get; set; }

        }

        //[Namespace = "http://bpc.ru/SVXP/integration")]
        public class GetCardsResponse
        {
            [XmlElement(ElementName = "card", Namespace = "http://bpc.ru/SVXP")]
            public Card Card { get; set; }
        }
        [XmlType(Namespace = "http://bpc.ru/SVXP")]
        public class Card
        {
            public string id { get; set; }
            public string card_instance_id { get; set; }
            public string card_mask { get; set; }
            public int seq_number { get; set; }
            public string expir_date { get; set; }
            public string person_name { get; set; }
            public string category { get; set; }
            public string network_id { get; set; }
            public string network_name { get; set; }
            public string card_number { get; set; }
            public string category_name { get; set; }
            public string card_type { get; set; }
            public string status { get; set; }
            public string status_name { get; set; }
            public string state { get; set; }
            public string delivery_status { get; set; }
            public string delivery_status_name { get; set; }
            public string state_name { get; set; }
            public string bin_currency { get; set; }
            public string currency_name { get; set; }
            public decimal aval_balance { get; set; }
            public decimal hold_balance { get; set; }
            public string customer_id { get; set; }
            public string customer_number { get; set; }
            public string inst_id { get; set; }
            public string account_number { get; set; }
            public string account_id { get; set; }
            public string contract_number { get; set; }
            public string product_id { get; set; }
            public string product_number { get; set; }
            public string product_name { get; set; }
            public DateTime creation_date { get; set; }
            public string embossed_name { get; set; }

            [XmlElement("cardholder_name")]
            public CardHolderName CardHolderName { get; set; }

            public string pos_account_id { get; set; }
            public string pos_account_number { get; set; }
            public string atm_account_id { get; set; }
            public string atm_account_number { get; set; }
            public int is_last_seq_number { get; set; }

        }

        public class CardHolderName
        {
            public string first_name { get; set; }
            public string last_name { get; set; }
        }
    }
}
