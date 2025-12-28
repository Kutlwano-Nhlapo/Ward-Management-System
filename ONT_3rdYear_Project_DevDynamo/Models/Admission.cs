using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class Admission
	{
		[Key]
		public int AdmissionID { get; set; }


		[Required]
		[ForeignKey("Patient")]
		public int PatientID { get; set; }


		[Required]
		[ForeignKey("Ward")]
		public int WardID { get; set; }
		[Required]
		[ForeignKey("ApplicationUser")]
		public int DoctorId { get; set; }

        [Required]
		[ForeignKey("Bed")]
		public int BedID { get; set; }

		[Required]
		[DataType(DataType.Date)]
        public DateOnly AdmissionDate { get; set; }

        public DateOnly? DischargeDate { get; set; }

        public string Notes { get; set; }
        [Required]
        public string  ReasonForAdmission { get; set; }

        public virtual Patient Patient { get; set; }
		public virtual Bed Bed { get; set; }
		public virtual Ward Ward { get; set; }
		public virtual ApplicationUser ApplicationUser { get; set; }
		public ICollection<PatientAllergy> PatientAllergies { get; set; }
		public ICollection<MedicalHistory> MedicalHistories { get; set; }
    }
}
