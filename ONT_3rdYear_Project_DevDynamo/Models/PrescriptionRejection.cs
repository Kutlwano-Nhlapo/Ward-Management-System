using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class PrescriptionRejection
	{
		[Key]
		public int RejectionID { get; set; }

		[ForeignKey("Prescription")]
		public int PrescriptionID { get; set; }
		public virtual Prescription Prescription { get; set; }

		[ForeignKey("User")]
		public int ApplicationUserID { get; set; }
		public virtual ApplicationUser User { get; set; }

		[MaxLength(250)]
		public string RejectionReason { get; set; }

		public DateTime? RejectionDate { get; set; }

	}
}
