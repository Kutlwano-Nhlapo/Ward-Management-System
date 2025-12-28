using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class Discharge
	{
		[Key]
		public int DischargeID { get; set; }

		[Required]
		[ForeignKey("User")]

		public int ApplicationUserID { get; set; }

		[ForeignKey("Patient")]
		public int PatientID { get; set; }

		[Required]
		public DateTime DischargeDate { get; set; }

		public string DischargeInstructions { get; set; }

		public bool? IsDischarged { get; set; }

		public virtual ApplicationUser User { get; set; }
		public virtual Patient Patient { get; set; }
	}
}
