using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ONT_3rdyear_Project.Models
{
	public class Movement
	{
		[Key]
		public int MovementID { get; set; }
		[Required]
		[ForeignKey("Patient")]
		public int PatientID { get; set; }
		[Required]
		[ForeignKey("Bed")]
		public int BedID { get; set; }
		[Required]
		[ForeignKey("Ward")]
		public int WardID { get; set; }
        [Required]
		public string FromLocation { get; set; }
		[Required]
		public string ToLocation { get; set; }
		[Required]
		public DateTime TimeStamp { get; set; }

		public virtual Patient Patient { get; set; }
		public virtual Bed Bed { get; set; }
		public virtual Ward Ward { get; set; }


    }
}
