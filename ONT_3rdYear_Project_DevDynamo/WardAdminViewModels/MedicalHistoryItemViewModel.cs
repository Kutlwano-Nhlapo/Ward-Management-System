using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.WardAdminViewModels
{
    public class MedicalHistoryItemViewModel
    {
        public string ChronicCondition { get; set; }

        public string MedicationHistory { get; set; }

        public string PastSurgicalHistory { get; set; }

        public DateTime RecorderDate { get; set; }

        public string ConditionSeverity { get; set; }
    }
}