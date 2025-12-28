using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class PrescriptionForwarding
	{
		[Key]
		public int ForwardingID { get; set; }

        public int PrescriptionID { get; set; }
        [ForeignKey("PrescriptionID")]
        public Prescription Prescription { get; set; }

        public int EmployeeID { get; set; }
        [ForeignKey("ApplicationUserID")]
        public ApplicationUser User { get; set; }


        public DateTime ForwardedDate { get; set; }

	}
}
