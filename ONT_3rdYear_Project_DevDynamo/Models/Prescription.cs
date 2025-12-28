using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class Prescription
	{
		[Key]
		public int PrescriptionId { get; set; }

		[ForeignKey("User")]
		public int ApplicationUserID { get; set; }
		public ApplicationUser User { get; set; }

		[ForeignKey("Patient")]
		public int PatientId { get; set; }
		public Patient Patient { get; set; }

		public DateTime DateIssued { get; set; }
        public string Status { get; set; } = "New";
        //public string PrescriptionInstruction { get; set; }


		public ICollection<PrescribeMedication> Prescribed_Medication { get; set; }
		public virtual ICollection<PrescriptionForwarding> PrescriptionForwarding { get; set; }
		public virtual ICollection<PrescriptionRejection> PrescriptionRejection { get; set; }
    }
}
