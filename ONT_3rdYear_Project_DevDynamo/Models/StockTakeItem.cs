using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
    public class StockTakeItem
    {
       
        [ForeignKey("StockTake")]
        public int StockTakeID { get; set; }
        public virtual StockTake StockTake { get; set; }
        [ForeignKey("Consumable")]
        public int ConsumableID { get; set; }
        public virtual Consumable Consumable { get; set; }
        public int QuantityCounted { get; set; }
        public int SystemQuantity { get; set; }
        public int Discrepancy { get; set; }
    }
}
