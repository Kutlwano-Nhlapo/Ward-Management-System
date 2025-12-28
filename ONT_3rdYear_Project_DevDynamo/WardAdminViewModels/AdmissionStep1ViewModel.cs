using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.WardAdminViewModels
{
    public class AdmissionStep1ViewModel
    {
        public int AdmissionID { get; set; }
        // Existing patient selection
        [Required(ErrorMessage = "Please select a patient.")]
        public int PatientID { get; set; }
        [Required]
        public int BedID { get; set; }
        [Required(ErrorMessage = "Please select a ward.")]
        public int WardID { get; set; }
        [Required]
        public int DoctorID { get; set; }
        [Required]
        public string ReasonForAdmission { get; set; }
        public string Notes { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly AdmissionDate { get; set; }

        public IEnumerable<SelectListItem> Patients { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Wards { get; set; } = new List<SelectListItem>();
    }   
}

