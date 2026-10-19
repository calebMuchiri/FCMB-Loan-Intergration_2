using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniApi.Models.Cards
{
    public enum CardStatusCode
    {
        Valid = 0,
        CallIssuer = 1,
        WarmCard = 2,
        DoNotHonor = 3,
        HonorWithId = 4,
        NotPermitted = 5,
        LostCardCapture = 6,
        StolenCardCapture = 7,
        CallSecurityCapture = 8,
        InvalidCardCapture = 9,
        PickUpCardSpecial = 10,
        CallAcquirerSecurity = 11,
        NotActivated = 12,
        PinAttemptsExceeded = 13,
        ForcedPinChange = 14,
        CreditDebts = 15,
        VirtualFree = 16,
        PinActivation = 17,
        PersonalizationWaiting = 18,
        FraudPrevention = 19,
        TemporaryBlockedByClient = 20
    }
}
