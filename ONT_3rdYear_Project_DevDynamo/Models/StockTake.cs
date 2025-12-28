using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
    public class StockTake
    {
        [Key]
        public int StockTakeID { get; set; }
        [ForeignKey("Ward")]
        public int WardID { get; set; }
        public virtual Ward Ward { get; set; }
        public DateTime StockTakeDate { get; set; }
        [ForeignKey(nameof(TakenBy))]
        public int TakenBy { get; set; }
        public virtual ApplicationUser TakenByUser { get; set; } 
        public ICollection<StockTakeItem> StockTakes { get; set; }
    }
}
