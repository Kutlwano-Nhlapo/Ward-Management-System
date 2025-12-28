using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace ONT_3rdyear_Project.Models
{
	public class TreatVisit
	{
		[Key]
		public int TreatVisitID { get; set; }

		[ForeignKey("User")]
		public int? ApplicationUserID { get; set; }

		[ForeignKey("Patient")]
		public int? PatientID { get; set; }
		public DateTime VisitDate { get; set; }
		public String ReasonForVisit { get; set; }
		public string? Notes { get; set; }
		public bool? IsCompleted { get; set; }


        public virtual ApplicationUser User { get; set; }
		public virtual Patient Patient { get; set; }
		public virtual ICollection<Instruction> Instructions { get; set; }
		public virtual ICollection<Treatment> Treatments { get; set; }
		public virtual ICollection<Vital> Vitals { get; set; }
		
	}
}
