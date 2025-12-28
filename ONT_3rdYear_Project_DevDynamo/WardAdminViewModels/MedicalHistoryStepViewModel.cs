using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.WardAdminViewModels
{
    public class MedicalHistoryStepViewModel
    {
        public int AdmissionID { get; set; }
        [Required]
        public int PatientID { get; set; }

        public List<MedicalHistoryItemViewModel> MedicalHistories { get; set; } = new List<MedicalHistoryItemViewModel>();
    }
}
