using System.ComponentModel.DataAnnotations;

namespace Document.Models
{
	public class LoginModel
	{
		[Required]
		[Display(Name = "User Name")]
		public string Username { get; set; }
		[Required]
		[Display(Name = "Password")]
		public string Password { get; set; }
	}
}
