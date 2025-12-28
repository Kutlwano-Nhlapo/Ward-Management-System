using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class Pharmacy
	{
		[Key]
		public int PharmacyID { get; set; }
		public string ContactInfo { get; set; }

		public ICollection<PharmacyMedication> PharmacyMedications { get; set; }
	}
}
