using System;

namespace OblivionAPI.Reports; 

[Serializable]
public class SalesReport_CollectionVolume {
    public uint ID { get; set; }
    public decimal Volume { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }

    public SalesReport_CollectionVolume(uint id, decimal volume, string name, string image) {
        ID = id;
        Volume = volume;
        Name = name;
        Image = image;
    }
}