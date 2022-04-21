namespace OblivionAPI.Reports {
    public class SalesReport_CollectionVolume {
        public uint ID { get; set; }
        public decimal Volume { get; set; }

        public SalesReport_CollectionVolume(uint id, decimal volume) {
            ID = id;
            Volume = volume;
        }
    }
}
