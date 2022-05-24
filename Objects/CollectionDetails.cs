/*
 *  OblivionAPI :: CollectionDetails
 *
 *  This class is used to store the details of a collection.
 * 
 */

namespace OblivionAPI.Objects; 

[Serializable]
public class CollectionDetails {
    public uint ID { get; set; }
    public string Owner { get; set; }
    public string Treasury { get; set; }
    public uint Royalties { get; set; }
    public uint CreateBlock { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public string Description { get; set; }
    public string Banner { get; set; }
    public string[] Nfts { get; set; }
    public DateTime LastRetrieved;

    public CollectionDetails(uint id, CollectionResponse response, string name, string image, string description, string banner, string[] nfts) {
        ID = id;
        Owner = response.Owner;
        Treasury = response.Treasury;
        Royalties = response.Royalties;
        CreateBlock = response.CreateBlock;
        Nfts = nfts;
        Name = name;
        Image = image;
        Banner = banner;
        Description = description;
        LastRetrieved = DateTime.Now;
    }

    public void Update(CollectionDetails details) {
        Owner = details.Owner;
        Treasury = details.Treasury;
        Royalties = details.Royalties;
        Nfts = details.Nfts;
        Name = details.Name;
        Image = details.Image;
        Banner = details.Banner;
        Description = details.Description;
        LastRetrieved = DateTime.Now;
    }
}