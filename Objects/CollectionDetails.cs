/*
 *  OblivionAPI :: CollectionDetails
 *
 *  This class is used to store the details of a collection.
 * 
 */

using OblivionAPI.Responses;
using System;

namespace OblivionAPI.Objects {
    [Serializable]
    public class CollectionDetails {
        public uint ID { get; set; }
        public string Owner { get; set; }
        public string Treasury { get; set; }
        public uint Royalties { get; set; }
        public uint CreateBlock { get; set; }
        public string[] Nfts { get; set; }
        public DateTime LastRetrieved;

        public CollectionDetails(uint id, CollectionResponse response, string[] nfts) {
            ID = id;
            Owner = response.Owner;
            Treasury = response.Treasury;
            Royalties = response.Royalties;
            CreateBlock = response.CreateBlock;
            Nfts = nfts;
            LastRetrieved = DateTime.Now;
        }

        public void Update(CollectionDetails details) {
            Treasury = details.Treasury;
            Royalties = details.Royalties;
            Nfts = details.Nfts;
            LastRetrieved = DateTime.Now;
        }
    }
}
