using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
