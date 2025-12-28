using Microsoft.AspNetCore.Mvc.Rendering;
using ONT_3rdyear_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.ViewModels
{
    public class TreatmentViewModel
    {
        public Treatment Treatment { get; set; }

        // Property for the custom "Other" treatment input
        
        public string TreatmentTypeOther { get; set; }

        
        public string PatientName { get; set; }

        public IEnumerable<SelectListItem> TreatmentTypes { get; set; }
        public IEnumerable<SelectListItem> VisitList { get; set; }
    }
}
