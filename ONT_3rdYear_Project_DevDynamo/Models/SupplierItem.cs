using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
    public class SupplierItem
    {
       
        [ForeignKey("Supplier")]
        public int SupplierID { get; set; }
        public virtual Supplier Supplier { get; set; }
        [ForeignKey("Consumable")]
        public int ConsumableID { get; set; }
        public virtual Consumable Consumable { get; set; }
        public int QuantityOnHand { get; set; }

    }
}
