using System;

namespace OmniApi.Models.FCMBLoan
{
    // Matches the expected Add Lien response:
    // {
    //   "status": true,
    //   "code": "00",
    //   "description": "SUCCESS",
    //   "data": {
    //     "lienId": "...",
    //     "accountId": "...",
    //     "lienAmount": 0.0,
    //     "lienDate": "2026-07-20T13:44:26.2698931+00:00"
    //   }
    // }
    public class AddLienResponse
    {
        public bool status { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public AddLienData data { get; set; }
    }

    public class AddLienData
    {
        public string lienId { get; set; }
        public string accountId { get; set; }

        // Make nullable because the 3rd-party may return null (fixes deserialization error)
        public decimal? lienAmount { get; set; }

        // Make nullable because the 3rd-party may return null
        public DateTimeOffset? lienDate { get; set; }
    }
}