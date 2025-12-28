using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace ONT_3rdyear_Project.SereroViewModels
{
    public class PrescribeMedicationViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Medications and Dosages")]
        public List<MedicationDosageItem> MedicationsWithDosage { get; set; } = new List<MedicationDosageItem>();

        [Display(Name = "Available Medications")]
        public List<SelectListItem> AvailableMedications { get; set; } = new List<SelectListItem>();

    }
}
