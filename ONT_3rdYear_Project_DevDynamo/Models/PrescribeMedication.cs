using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class PrescribeMedication
	{
		[Key]
		public int PrescribedMedicationId { get; set; }

		[ForeignKey("Prescription")]
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; }

		[ForeignKey("Medication")]
        public int MedicationId { get; set; }
        public Medication Medication { get; set; }

		public string Dosage { get; set; }
	}
}
