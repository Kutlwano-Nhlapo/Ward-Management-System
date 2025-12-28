using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.ViewModels
{
    public class StockTakeViewModel
    {
        [Required]
        public int WardID { get; set; }

        public int TakenByID { get; set; }

        public List<StockTakeItemEntry> Items { get; set; }

        //public List<SelectListItem> Wards { get; set; }
    }

    public class StockTakeItemEntry
    {

        public int ConsumableID { get; set; }
        public string ConsumableName { get; set; }
        public int SystemQuantity { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int CountedQuantity { get; set; }
    }
}
