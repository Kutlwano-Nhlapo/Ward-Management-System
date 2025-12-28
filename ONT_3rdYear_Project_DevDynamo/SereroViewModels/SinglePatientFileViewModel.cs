using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.SereroViewModels
{
    public class SinglePatientFileViewModel
    {
        //[Key]
        //public int PatientID { get; set; }
        //[Required]
        //[StringLength(50, ErrorMessage = "Name must not be greater than be 50 charecters")]
        //public string FirstName { get; set; }
        //[Required]
        //[StringLength(50, ErrorMessage = "Last Name must not be greater than be 50 charecters")]
        //public string LastName { get; set; }
        // Patient Info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ChronicIllness { get; set; }
        public bool Admitted { get; set; }

        // Medical History
        public string ChronicCondition { get; set; }
        public string MedicationHistory { get; set; }
        public string PastSurgicalHistory { get; set; }
        public DateTime RecorderDate { get; set; }
        public string ConditonSeverity { get; set; }

        // Vitals
        public string BP { get; set; }
        public double Temperature { get; set; }
        public string SugarLevel { get; set; } // Converted to string for display
        public string PulseRate { get; set; }  // Converted to string for display
        public DateTime Date { get; set; }

        // Treatment
        public string TreatmentType { get; set; }
        public DateTime TreatmentDate { get; set; }

        // Visit Feedback
        public string VisitFeedback { get; set; }
        public DateTime? NextVisit { get; set; }

        // Flags
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }
}
