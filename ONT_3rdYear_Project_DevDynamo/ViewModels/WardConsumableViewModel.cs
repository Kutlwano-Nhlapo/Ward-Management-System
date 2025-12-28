namespace ONT_3rdyear_Project.ViewModels
{
    public class WardConsumableViewModel
    {
        public int WardID { get; set; }
        public int ConsumableID { get; set; }
        public string WardName { get; set; }
        public string ConsumableName { get; set; }
        public string Category { get; set; }
        public int QuantityCounted { get; set; }
        public int SystemQuantity {get; set; }
        public int Discrepancy { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
