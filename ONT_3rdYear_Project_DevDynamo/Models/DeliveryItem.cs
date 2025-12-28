using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
    public class DeliveryItem
    {
        
        [ForeignKey(nameof(DeliveryID))]
        public int DeliveryID { get; set; }
        public virtual Delivery Delivery { get; set; }
        [ForeignKey(nameof(ConsumableID))]
        public int ConsumableID { get; set; }
        public virtual Consumable Consumable { get; set; }
        public int QuantityDelivered { get; set; }
    }
}
