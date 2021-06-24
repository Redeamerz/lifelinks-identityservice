using Identity_Service.AccountConfig;
using Identity_Service.Models;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
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
		private readonly IEventService eventService;
		private readonly SignInManager<AppUser> signInManager;

		public AccountController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, IEventService eventService, SignInManager<AppUser> signInManager)
		{
			this.roleManager = roleManager;
			this.userManager = userManager;
			this.eventService = eventService;
			this.signInManager = signInManager;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
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
			await userManager.AddClaimAsync(user, new Claim("role", role));

			return Created("api.lifelinks.nl/api/account/register", user);
		}

		[HttpPut("Update")]
		public async Task<IActionResult> UpdateAccount([FromBody] UpdateModel model)
		{
			if (!ModelState.IsValid) return BadRequest(model);

			var user = await userManager.FindByIdAsync(model.Id);

			if (user == null)
			{
				return BadRequest();
			}

			if (user.NormalizedEmail != model.Email.Normalize())
			{
				var token = await userManager.GenerateChangeEmailTokenAsync(user, model.Email);
				var result = await userManager.ChangeEmailAsync(user, model.Email, token);
			}
			if (user.NormalizedUserName != model.Username.Normalize())
			{
				await userManager.SetUserNameAsync(user, model.Username);
			}
			return NoContent();
		}

		[HttpPost("SignIn")]
		public async Task<IActionResult> SignIn([FromBody] LoginModel model)
		{
			var user = await signInManager.UserManager.FindByNameAsync(model.Username);

			if (user == null)
			{
				return BadRequest();
			}

			if ((await signInManager.CheckPasswordSignInAsync(user, model.Password, false)) == Microsoft.AspNetCore.Identity.SignInResult.Failed)
			{
				return BadRequest();
			}

			await eventService.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.Email));

			AuthenticationProperties props = null;
			if (AccountOptions.AllowRememberLogin && model.RememberLogin)
			{
				props = new AuthenticationProperties
				{
					IsPersistent = true,
					ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
				};
			}

			var issuer = new IdentityServerUser(user.Id)
			{
				DisplayName = user.UserName
			};

			await HttpContext.SignInAsync(issuer, props);

			return Ok(issuer);
		}

		[HttpPost("SignOut")]
		public async Task<IActionResult> Logout()
		{
			if (User?.Identity.IsAuthenticated != true)
			{
				return BadRequest();
			}

			await signInManager.SignOutAsync();

			await eventService.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));

			return SignOut();
		}

		[Authorize]
		[HttpGet("check")]
		public IActionResult CheckAuth()
		{
			var check = User.Identity.IsAuthenticated;
			return Ok(check);
		}
	}
}