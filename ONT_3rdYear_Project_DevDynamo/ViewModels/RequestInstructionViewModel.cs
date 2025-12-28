using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.ViewModels
{
    public class RequestInstructionViewModel
    {
        public int PatientID { get; set; }
        public int VisitID { get; set; }
        public string PatientName { get; set; } 

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }
    }

}
