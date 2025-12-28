using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class Supplier
	{
		[Key]
		public int SupplierId { get; set; }

		[Required]
		public string SupplierName { get; set; }
		[Required]
		public DateOnly SuppliedDate { get; set; }

		public ICollection<Order> Orders { get; set; }
		public virtual ICollection<SupplierItem> SupplierItems { get; set; }
    }
}
