using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class PharmacyMedication
	{
		[Key]
		public int PhamarcyMedId { get; set; }


		[ForeignKey("Phamarcy")]
		public int PhamarcyID { get; set; }
		[ForeignKey("Medication")]
		public int MedicationID { get; set; }

		[Required(ErrorMessage = "Medication Name is required")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Dosage is required")]
		public string Dosage { get; set; }

		[Required]
		public string Schedule { get; set; }
		public DateOnly ExpiryDate { get; set; }

		public virtual Pharmacy pharmacy { get; set; }
		public virtual Medication medication { get; set; }
	}
}
