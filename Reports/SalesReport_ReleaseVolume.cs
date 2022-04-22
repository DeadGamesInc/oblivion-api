namespace OblivionAPI.Reports {
    public class SalesReport_ReleaseVolume {
        public uint ID { get; set; }
        public decimal Volume { get; set; }
        
        public SalesReport_ReleaseVolume(uint id, decimal volume) {
            ID = id;
            Volume = volume;
        }
    }
}
