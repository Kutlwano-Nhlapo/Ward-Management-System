using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class PatientAllergy
	{

        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }

		[ForeignKey("Allergy")]
        public int AllergyId { get; set; }
        public virtual Allergy Allergy { get; set; }
		[ForeignKey("Admission")]
		public int? AdmissionId { get; set; }
		public virtual Admission Admission { get; set; }
		public string Notes { get; set; }
        [Required]
		public string Severity { get; set; }
	}
}
