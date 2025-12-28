 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class Treatment
	{
		[Key]
		public int TreatmentID { get; set; }

		
		[ForeignKey("VisitSchedule")]//visitschedule
		public int? VisitID { get; set; }


		[Required]
		[ForeignKey("User")]
		public int ApplicationUserID { get; set; }


		[Required]
		[ForeignKey("Patient")]
		public int PatientID { get; set; }


		
		[ForeignKey("Treat_Visit")]
		public int? Treat_VisitID { get; set; }


		[Required]
		public string TreatmentType { get; set; }

		[Required]//added 
        public DateTime TreatmentDate { get; set; }//datetime

        public bool IsActive { get; set; } = true;


        public virtual VisitSchedule VisitSchedule { get; set; }
		public virtual ApplicationUser User { get; set; }
		public virtual Patient Patient { get; set; }
		public virtual TreatVisit TreatVisit { get; set; }
	}
}
