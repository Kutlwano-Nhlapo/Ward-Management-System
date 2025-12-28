namespace ONT_3rdyear_Project.ViewModels
{
    public class PatientDashboardViewModel
    {
        public int PatientID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string WardName { get; set; }
        public string BedNo { get; set; }
        public string Status { get; set; }
        public DateOnly? AdmissionDate { get; set; }
        public bool HasNewAdvice { get; set; }
        public DateTime? LastAdviceCheck { get; set; }
        public int UnreadAdviceCount { get; set; }

    }
}
