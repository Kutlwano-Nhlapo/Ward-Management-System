using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.SereroViewModels
{
    public class PatientInfoViewModel
    {
        [Key]
        public int PatientID { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name must not be greater than be 50 charecters")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Last Name must not be greater than be 50 charecters")]
        public string LastName { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string BedNo { get; set; }
    }
}
