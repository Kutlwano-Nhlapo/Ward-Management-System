using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class Allergy
	{
		[Key]
		public int AllergyId { get; set; }
		[Required(ErrorMessage = "Allergy name is required")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Allergy description is required")]
		public string Description { get; set; }

		public bool IsDeleted { get; set; } = false;

		public virtual ICollection<PatientAllergy> PatientAllergies { get; set; }

	}
}
