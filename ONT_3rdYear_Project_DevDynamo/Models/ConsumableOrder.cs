using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class ConsumableOrder
	{
		

		[ForeignKey("Consumable")]
		public int ConsumableId { get; set; }
		public virtual Consumable Consumable { get; set; }

		[ForeignKey("Order")]
		public int OrderId { get; set; }
		public virtual Order Order { get; set; }
        public int QuantityRequested { get; set; }
        public int QuantityApproved { get; set; }
        public string Reason { get; set; }
    }
}
