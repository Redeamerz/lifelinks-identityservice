using System.ComponentModel.DataAnnotations;

namespace Identity_Service.Models
{
	public class LoginModel
	{
		/// <summary>
		/// Model to verify incoming Account login request
		/// </summary>
		[Required]
		[Display(Name = "Username")]
		public string Username { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Text)]
		public bool RememberLogin { get; set; }
	}
}