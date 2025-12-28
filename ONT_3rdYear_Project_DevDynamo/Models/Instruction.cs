using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
    public class Instruction
    {
        [Key]
        public int InstructionID { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientID { get; set; }

        [Required]
        [ForeignKey("User")]
        public int ApplicationUserID { get; set; }

        [ForeignKey("TreatVisit")]
        public int? TreatVisitID { get; set; }

        [ForeignKey("VisitSchedule")]
        public int? VisitID { get; set; }

        
        public string? NurseRequest { get; set; }

        
        public string Instructions { get; set; }


        public DateTime? RespondedAt { get; set; } 

        [Required]
        public DateTime DateRecorded { get; set; }
        public bool isActive { get; set; } = true;

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual TreatVisit TreatVisit { get; set; }
        public virtual VisitSchedule VisitSchedule { get; set; }
        public virtual Patient Patient { get; set; }
    }


}
