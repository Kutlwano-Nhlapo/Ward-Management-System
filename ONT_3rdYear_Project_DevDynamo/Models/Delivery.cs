using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryID { get; set; }
        [ForeignKey("Order")]
        public int OrderID { get; set; }
        public virtual Order Order { get; set; }
        public DateTime DeliveryDate { get; set; }
        [ForeignKey(nameof(DeliveredBy))]
        public int DeliveredBy { get; set; }
        public virtual ApplicationUser DeliveredByUser { get; set; }
        [ForeignKey(nameof(RecievedBy))]
        public int RecievedBy { get; set; }
        public virtual ApplicationUser RecievedByUser { get; set; }
        public ICollection<DeliveryItem> DeliveryItems { get; set; }
    }
}
