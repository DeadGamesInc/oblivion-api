/*
 *  OblivionAPI :: OfferDetails
 *
 *  This class is used to store the details of an offer made on a listing on the market.
 * 
 */

using OblivionAPI.Responses;
using System;

namespace OblivionAPI.Objects {
    public class OfferDetails {
        public string PaymentToken { get; set; }
        public uint ID { get; set; }
        public DateTime LastRetrieved;
        public string Offeror { get; set; }
        public string Amount { get; set; }
        public string Discount { get; set; }
        public bool Claimed { get; set; }
        public string CreateBlock { get; set; }
        public string EndBlock { get; set; }

        public OfferDetails(string paymentToken, uint id, OfferResponse response) {
            PaymentToken = paymentToken;
            ID = id;
            Offeror = response.Offeror;
            Amount = response.Amount.ToString();
            Discount = response.Discount.ToString();
            Claimed = response.Claimed;
            CreateBlock = response.CreateBlock.ToString();
            EndBlock = response.EndBlock.ToString();
            LastRetrieved = DateTime.Now;
        }

        public void Update(OfferDetails response) {
            Claimed = response.Claimed;
            EndBlock = response.EndBlock;
            LastRetrieved = DateTime.Now;
        }
    }
}
