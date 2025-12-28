using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
	public class ApplicationUser : IdentityUser<int>
	{

		[Required]
		[StringLength(50)]
		public string FullName { get; set; }

		public string RoleType { get; set; } // Admin, Nurse, Doctor, etc.

		public bool IsDeleted { get; set; } = false;

		// Optional relationships
		public virtual ICollection<Treatment> Treatments { get; set; }
		public virtual ICollection<Instruction> Instructions { get; set; }
		public virtual ICollection<Vital> Vitals { get; set; }
		public virtual ICollection<Discharge> Discharges { get; set; }
		public virtual ICollection<Admission> Admissions { get; set; }
		public virtual ICollection<PatientAllergy> PatientAllergies { get; set; }
		public virtual ICollection<MedicalHistory> MedicalHistories { get; set; }
		public virtual ICollection<Prescription> Prescriptions { get; set; }
		public virtual ICollection<Movement> Movements { get; set; }
		public virtual ICollection<VisitSchedule> VisitSchedules { get; set; }
		public virtual ICollection<PatientMedicationScript> AdministraterMedications { get; set; }
		public virtual ICollection<Ward> Wards { get; set; }
		public virtual ICollection<TreatVisit> TreatVisits { get; set; }
		public virtual ICollection<DoctorAssignment> DoctorAssignments { get; set; }
		public virtual ICollection<Patient> Patients { get; set; } // For doctors and nurses who may have patients assigned
	}
}
