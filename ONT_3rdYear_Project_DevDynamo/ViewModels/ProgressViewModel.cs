using Microsoft.AspNetCore.Mvc.Rendering;
using ONT_3rdyear_Project.Models;

namespace ONT_3rdyear_Project.ViewModels
{
    public class ProgressViewModel
    {
        public int? SelectedPatientId { get; set; }
        public SelectList PatientsList { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // raw lists (optional, useful for tables)
        public List<Vital> Vitals { get; set; }
        public List<PatientMedicationScript> Medications { get; set; }
        public List<Treatment> Treatments { get; set; }

        // JSON prepared for the view/chart
        public string ChartLabelsJson { get; set; }
        public string BpJson { get; set; }
        public string PulseJson { get; set; }
        public string SugarJson { get; set; }
        public string TempJson { get; set; }

        // medication markers (numeric array aligned with labels) and tooltips (array of arrays)
        public string MedDataJson { get; set; }
        public string MedTooltipsJson { get; set; }

        // treatments markers
        public string TreatmentDataJson { get; set; }
        public string TreatmentTooltipsJson { get; set; }
        public string PatientName { get; set; }
        // Add these properties to your ProgressViewModel
        public string CompanyName { get; set; } = "Your Healthcare Facility";
        public string ReportTitle { get; set; } = "Patient Progress Report";
        public string ReportPeriod { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string CompanyAddress { get; set; } = "123 Medical Center, Healthcare City, 10001";
        public string CompanyContact { get; set; } = "Tel: (555) 123-4567 | Email: info@healthcarefacility.com";
    }
}
