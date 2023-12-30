using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Todo_App.Configuration;
using Todo_App.Models.DTOs.Requests;
using Todo_App.Models.DTOs.Responses;

namespace Todo_App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthManagementController : ControllerBase
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly JwtConfig _jwtConfig;

	public AuthManagementController(
		UserManager<IdentityUser> userManager, 
		IOptionsMonitor<JwtConfig> optionsMonitor
	)
	{
		_userManager = userManager;
		_jwtConfig = optionsMonitor.CurrentValue;
	}


	[HttpPost]
	[Route("Register")]
	public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
	{
		if (ModelState.IsValid)
		{
			// we can utilise the model
			var existingUser = await _userManager.FindByEmailAsync(user.Email);
			if (existingUser != null)
			{
				return BadRequest(new RegistrationResponseDto()
				{
					Errors = new List<string>()
					{
						"Email already in use"
					},
					Success = false
				});
			}
		}

		var newUser = new IdentityUser()
		{
			Email = user.Email,
			UserName = user.UserName
		};

		var isCreated = await _userManager.CreateAsync(newUser, user.Password);
		if (isCreated.Succeeded)
		{
			var jwtToken = GenerateJwtToken(newUser);


			// Return the token to the user
			return Ok(new RegistrationResponseDto()
			{
				Success = true,
				Token = jwtToken,
			});
		}
		else
		{
			return BadRequest(new RegistrationResponseDto()
			{
				
				Errors = isCreated.Errors.Select(x => x.Description).ToList(),
				Success = false
			});
		}

		return BadRequest(new RegistrationResponseDto()
		{
			Errors = new List<string>()
			{
				"Invalid payload"
			}
		});
	}



	private string GenerateJwtToken(IdentityUser user)
	{
		// Create the Jwt handler
		// The handler is going to be responsible for creating the token
		var jwtHandler = new JwtSecurityTokenHandler();

		// Get the security key
		var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[]
			{
				new Claim("Id", user.Id),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(JwtRegisteredClaimNames.Sub, user.Email), // unique id
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // used by the refresh token
			}),
			Expires = DateTime.UtcNow.AddHours(6), //todo: update the expiration time to minutes
			SigningCredentials = new SigningCredentials
			(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature // todo: review the algorithm down the road
			)
		};

		// Generate the security token 
		var token = jwtHandler.CreateToken(tokenDescriptor);

		// Convert the security obj token into a string
		var jwtToken = jwtHandler.WriteToken(token);

		return jwtToken;
	}
}
