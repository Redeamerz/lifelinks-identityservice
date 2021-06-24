using System.ComponentModel.DataAnnotations;

namespace Identity_Service.Models
{
	public class UpdateModel
	{
		[Required]
		[Display(Name = "Guid")]
		public string Id { get; set; }

		[Required]
		[Display(Name = "Username")]
		public string Username { get; set; }

		[Required]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "Email")]
		public string Email { get; set; }
	}
}