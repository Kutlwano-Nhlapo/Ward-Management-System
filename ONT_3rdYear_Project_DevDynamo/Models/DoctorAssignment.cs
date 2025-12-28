using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
    public class DoctorAssignment
    {
        [Key]
        public int AssignmentID { get; set; }

        [Required]
        [ForeignKey("ApplicationUser")]
        public int DoctorID { get; set; } // Foreign key to ApplicationUser (Doctor)

        [Required]
        [ForeignKey("Patient")]
        public int PatientID { get; set; } // Foreign key to Patient

        public DateTime AssignmentDate { get; set; } // Date of assignment

        public DateTime? UnassignedDate { get; set; } // Optional date when the doctor was unassigned

        [Required]
        public bool IsActive { get; set; } = true; // Indicates if the assignment is currently active   

        public virtual ApplicationUser ApplicationUser { get; set; } // Navigation property to ApplicationUser (Doctor)
        public virtual Patient Patient { get; set; } // Navigation property to Patient
    }
}
