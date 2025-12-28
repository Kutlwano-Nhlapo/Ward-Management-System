using System.ComponentModel.DataAnnotations;

namespace ONT_3rdyear_Project.Models
{
    public class HospitalInfo
    {
        [Key]
        public int HospitalInfoId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Phone]
        public string Phone { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public string Description { get; set; }

        public string Website {  get; set; }

        public string DirectorName { get; set; }

        public string? Logo { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
