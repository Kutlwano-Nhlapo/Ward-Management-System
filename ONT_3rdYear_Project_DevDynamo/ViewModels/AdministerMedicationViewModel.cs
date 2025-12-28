using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.ViewModels
{
    public class AdministerMedicationViewModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }

        public string PatientName { get; set; }

        [Required]
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }

        public SelectList MedicationList { get; set; }

        [Required]
        public string Dosage { get; set; }

        [Required]
        public DateTime AdministeredDate { get; set; }

        [Required]
        public int ApplicationUserID { get; set; }
        public string ApplicationUserName { get; set; }
        public SelectList UserList { get; set; }

        public string? PrescriptionNote { get; set; } // Optional for high schedule meds
        public int? PrescriptionId { get; set; }  // nullable, because not all meds may need prescription
        public SelectList PrescriptionList { get; set; }
        public bool RequiresPrescription { get; set; }
        public bool HasPrescription => PrescriptionId.HasValue;
        public List<string> PatientAllergies { get; set; } // List of allergy names
        public string? PrescriptionDosage { get; set; }
    }

}
