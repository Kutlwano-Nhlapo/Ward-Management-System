using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class VisitSchedule
	{
		[Key]
		public int VisitID { get; set; }
		[Required]
		[ForeignKey("User")]
		public int ApplicationUserID { get; set; }

		[Required]
		[ForeignKey("Patient")]
		public int PatientID { get; set; }

		[Required]
		public DateTime VisitDate { get; set; }

		[Required]
		public string Feedback { get; set; }
		public DateTime? NextVisit { get; set; }

        public bool IsActive { get; set; } = true;


        public virtual ApplicationUser User { get; set; }
		public virtual Patient Patient { get; set; }
		public virtual ICollection<Instruction> Instructions { get; set; }
        public virtual ICollection<Vital> Vitals { get; set; }
		public virtual ICollection<Treatment> Treatments { get; set; }
        public virtual ICollection<PatientMedicationScript> PatientMedicationScripts { get; set; }


    }
}
