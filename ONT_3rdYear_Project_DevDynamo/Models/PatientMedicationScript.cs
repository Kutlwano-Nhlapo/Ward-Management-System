using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class PatientMedicationScript
	{
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }

        [Required(ErrorMessage="Medication Is required")]
        [ForeignKey("Medication")]
        public int MedicationId { get; set; }
        public virtual Medication Medication { get; set; }

        [ForeignKey("VisitID")]
        public int? VisitID { get; set; }
        public virtual VisitSchedule VisitSchedule { get; set; }

        [Required]
        [ForeignKey("AdministeredBy")]
        public int ApplicationUserID{ get; set; } // Administered by
        public virtual ApplicationUser AdministeredBy { get; set; }

        public int? PrescriptionId { get; set; }
        public virtual Prescription Prescription { get; set; }

        [Required(ErrorMessage ="Date is required")]
        public DateTime AdministeredDate { get; set; }

        [Required]
        public string Dosage { get; set; }

        public bool isActive { get; set; } = true;


    }
}
