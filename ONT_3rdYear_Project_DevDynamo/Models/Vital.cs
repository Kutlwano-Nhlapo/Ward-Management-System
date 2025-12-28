using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ONT_3rdyear_Project.Models
{
	public class Vital
	{
		[Key]
		public int VitalID { get; set; }
		
		[ForeignKey("VisitSchedule")]
		public int? VisitID {  get; set; }


		[Required]
		[ForeignKey("User")]
		public int ApplicationUserID { get; set; }


		[Required]
		[ForeignKey("Patient")]
		public int PatientID { get; set; }


		[Required]
		public string BP {  get; set; }


		[Required]
        public double Temperature { get; set; }


		[Required]
        [Range(1.0, 30.0, ErrorMessage = "Sugar level must be a realistic value.")]
        public double SugarLevel { get; set; }


        [Required]
        [Range(30, 220, ErrorMessage = "Pulse rate must be between 30 and 220.")]
        public int PulseRate { get; set; }

        [Required]
		public DateTime Date { get; set; }

		public bool IsActive { get; set; } = true;


        public virtual VisitSchedule VisitSchedule { get; set; }
		public virtual Patient Patient { get; set; }
		public virtual ApplicationUser User { get; set; }
		
	}
}
