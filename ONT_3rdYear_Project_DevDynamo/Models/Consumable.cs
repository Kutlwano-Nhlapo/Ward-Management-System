using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class Consumable
	{
		[Key]
		public int ConsumableId { get; set; }

		[Required(ErrorMessage = "Name is required")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Category is required")]
		public string Category { get; set; }

		[Required(ErrorMessage = "Stock Quantity is required")]
		public int StockQuantity { get; set; }      

		public bool IsDeleted { get; set; } = false;
        public virtual ICollection<ConsumableOrder> ConsumableOrders { get; set; }
        public virtual ICollection<WardConsumable> WardConsumables { get; set; }
		public virtual ICollection<DeliveryItem> DeliveryItems { get; set; }
		public virtual ICollection<SupplierItem> SupplierItems { get; set; }
		public virtual ICollection<StockTakeItem> StockTakeItems { get; set; }
       

    }
}
