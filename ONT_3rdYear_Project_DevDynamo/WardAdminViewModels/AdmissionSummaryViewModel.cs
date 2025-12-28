using ONT_3rdyear_Project.Models;

namespace ONT_3rdyear_Project.WardAdminViewModels
{
    public class AdmissionSummaryViewModel
    {
        public AdmissionStep1ViewModel Step1 { get; set; }
        public AllergiesStepViewModel Step2 { get; set; }
        public MedicalHistoryStepViewModel Step3 { get; set; }

        public string PatientName { get; set; }
        public string WardName { get; set; }
        public string BedNo { get; set; }
        public string DoctorName { get; set; }

    }
}
