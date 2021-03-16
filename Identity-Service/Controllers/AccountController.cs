using Identity_Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity_Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly UserManager<AppUser> userManager;

		public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.roleManager = roleManager;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
		{
			if (!ModelState.IsValid) return BadRequest(model);

			var user = new AppUser { UserName = model.UserName, Email = model.Email };

			var result = await userManager.CreateAsync(user, model.Password);

			string role = "Basic User";

			if (!result.Succeeded) return BadRequest(model);

			if (await roleManager.FindByNameAsync(role) == null)
			{
				await roleManager.CreateAsync(new IdentityRole(role));
			}

			await userManager.AddToRoleAsync(user, role);
			await userManager.AddClaimAsync(user, new Claim("userName", user.UserName));
			await userManager.AddClaimAsync(user, new Claim("email", user.Email));
			await userManager.AddClaimAsync(user, new Claim("role", role));

			return Created("api.lifelinks.nl/api/account/register", user);
		}
	}
}
