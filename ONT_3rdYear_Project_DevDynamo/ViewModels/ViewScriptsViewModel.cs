using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.ViewModels
{
    public class ViewScriptsViewModel
    {
        public int PatientID { get; set; }
        public int? PrescriptionId { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Ward { get; set; }
        public string DateCreated { get; set; }
        public List<int> Schedule { get; set; }
        public string Status { get; set; }       
        public List<string> Dosage { get; set; }
        public List<string> medications { get; set; }
        public int Duration { get; set; }

    }
    
}
