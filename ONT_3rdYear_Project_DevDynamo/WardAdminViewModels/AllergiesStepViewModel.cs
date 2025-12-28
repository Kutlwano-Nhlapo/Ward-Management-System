using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.WardAdminViewModels
{
    public class AllergyEntry
    {
        public int SelectedAllergyId { get; set; }
        public string OtherAllergyName { get; set; }
        public string OtherAllergyDescription { get; set; }
        public string Severity { get; set; }
        public string Notes { get; set; }
        public string ResolvedName { get; set; }
    }

    public class AllergiesStepViewModel
    {
        public int AdmissionID { get; set; }
        public int PatientID { get; set; }

        // Multiple rows
        public List<AllergyEntry> Allergies { get; set; } = new List<AllergyEntry>();

        // For dropdown options
        public IEnumerable<SelectListItem> AllergyOptions { get; set; } = new List<SelectListItem>();
    }
}

