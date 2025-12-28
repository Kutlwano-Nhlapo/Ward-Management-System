using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.ViewModels
{
    public class CreateOrderViewModel
    {
        public int WardID { get; set; }
        public string Remarks { get; set; }

        public List<OrderItemViewModel> RequestItems { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ConsumableID { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
        public int QuantityRequested { get; set; }

        public string Reason { get; set; }
    }
}
