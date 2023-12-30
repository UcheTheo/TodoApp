using System.ComponentModel.DataAnnotations;

namespace Todo_App.Models.DTOs.Requests;

public class UserRegistrationDto 
{
	public string UserName { get; set; }

	[Required]
	[EmailAddress]
	public string Email { get; set; }

	[Required]
	public string Password { get; set; }
}
