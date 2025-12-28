using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.SereroViewModels
{
    public class MedicationDosageItem
    {
        [Required]
        [Display(Name = "Medication")]
        public int MedicationId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Dosage cannot exceed 500 characters")]
        [Display(Name = "Dosage Instructions")]
        public string Dosage { get; set; } = string.Empty;

        // Optional: For display purposes in views
        public string? MedicationName { get; set; }
    }
}
