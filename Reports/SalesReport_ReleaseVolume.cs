using System;

namespace OblivionAPI.Reports {
    [Serializable]
    public class ReleaseCollection {
        public uint ID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        
        public ReleaseCollection(uint id, string name, string image) {
            ID = id;
            Name = name;
            Image = image;
        }
    }

    [Serializable]
    public class SalesReport_ReleaseVolume {
        public uint ID { get; set; }
        public decimal Volume { get; set; }
        public ReleaseCollection? Collection { get; set; }

        public SalesReport_ReleaseVolume(uint id, decimal volume, ReleaseCollection? collection) {
            ID = id;
            Volume = volume;
            Collection = collection;
        }
    }
}
