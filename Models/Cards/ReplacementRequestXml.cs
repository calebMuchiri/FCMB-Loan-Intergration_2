using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public class ReplacementRequestXml
    {
        public string ApplicationType { get; set; }
        public string ApplicationFlowID { get; set; }
        public string ApplicationStatus { get; set; }
        public string OperatorID { get; set; }
        public string InstitutionID { get; set; }
        public string AgentID { get; set; }
        public string CustomerType { get; set; }
        public Customer customer { get; set; }

    }

    public class Customer
    {
        public string CustomerCommand { get; set; }
        public string CustomerNumber { get; set; }
        public Contract Contract { get; set; }
    }
    public class Contract
    {
        public string ContractCommand { get; set; }
        public Card Card { get; set; }
    }
    public class Card
    {
        public string CardCommand { get; set; }
        public string CardID { get; set; }
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public string Category { get; set; }
        public string CardDeliveryStatus { get; set; }
        public string PinRequest { get; set; }
        public string PersoPriority { get; set; }
        public string PinMailerRequest { get; set; }
        public string ReIssueCommand { get; set; }
        public string ReIssueCardState { get; set; }
        public string ReIssueProductID { get; set; }
        public string SequentialNumber { get; set; }
        public string CardState { get; set; }
        public string CardStatus { get; set; }
        public Cardholder cardholder { get; set; }
    }
    public class Cardholder
    {
        public string CardHolderCommand { get; set; }
        public string CardHolderName { get; set; }
        public string CustomerRelation { get; set; }
    }
}
