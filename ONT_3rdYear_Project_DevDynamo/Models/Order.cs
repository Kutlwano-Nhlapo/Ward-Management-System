using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class Order
	{
		[Key]
		public int OrderID { get; set; }
		[ForeignKey("User")]
		public int OrderedBy { get; set; }
		public virtual ApplicationUser User { get; set; }
		[ForeignKey("Ward")]
        public int WardID { get; set; }
		public virtual Ward Ward { get; set; }
        [Required]
		[ForeignKey("Supplier")]
		public int SupplierID { get; set; }
		[Required]
		public DateTime OrderDate { get; set; }
		[Required]
		public int OrderQuantity { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<ConsumableOrder> ConsumableOrders { get; set; }
    }
}
