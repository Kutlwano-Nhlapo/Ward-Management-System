using Microsoft.AspNetCore.Mvc.Rendering;

namespace ONT_3rdyear_Project.StaticHelper
{
    public class ConsumableCategoryHelper
    {
        public static List<SelectListItem> GetCategories()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Protective Gear", Text = "Protective Gear" },
                new SelectListItem { Value = "Injection Supplies", Text = "Injection Supplies" },
                new SelectListItem { Value = "Infusion Equipment", Text = "Infusion Equipment" },
                new SelectListItem { Value = "Linen Supplies", Text = "Linen Supplies" },
                new SelectListItem { Value = "Diagnostics", Text = "Diagnostics" },
                new SelectListItem { Value = "Wound Care", Text = "Wound Care" },
                new SelectListItem { Value = "Disinfection", Text = "Disinfection" },
                new SelectListItem { Value = "Surgical Supplies", Text = "Surgical Supplies" },
                new SelectListItem { Value = "Respiratory Supplies", Text = "Respiratory Supplies" },
                new SelectListItem { Value = "Patient Care Supplies", Text = "Patient Care Supplies" },
                new SelectListItem { Value = "Personal Hygiene", Text = "Personal Hygiene" },
                new SelectListItem { Value = "Laboratory Supplies", Text = "Laboratory Supplies" },
                new SelectListItem { Value = "IV Accessories", Text = "IV Accessories" },
                new SelectListItem { Value = "Monitoring Equipment", Text = "Monitoring Equipment" },
                new SelectListItem { Value = "Dressings & Bandages", Text = "Dressings & Bandages" },
                new SelectListItem { Value = "Pharmaceuticals", Text = "Pharmaceuticals" },
                new SelectListItem { Value = "Cleaning Supplies", Text = "Cleaning Supplies" },
                new SelectListItem { Value = "Gloves", Text = "Gloves" },
                new SelectListItem { Value = "Masks & Respirators", Text = "Masks & Respirators" },
                new SelectListItem { Value = "Catheters & Tubing", Text = "Catheters & Tubing" },
                new SelectListItem { Value = "Sharps Disposal", Text = "Sharps Disposal" },
                new SelectListItem { Value = "Syringes & Needles", Text = "Syringes & Needles" },
                new SelectListItem { Value = "Oxygen Supplies", Text = "Oxygen Supplies" },
                new SelectListItem { Value = "Emergency Equipment", Text = "Emergency Equipment" },
                new SelectListItem { Value = "Medical Gases", Text = "Medical Gases" }
            };
        }
    }
}
