using Identity_Service.Models;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity_Service
{
	public class IdentityClaimsProfileService : IProfileService
	{
		private readonly IUserClaimsPrincipalFactory<AppUser> claimsFactory;
		private readonly UserManager<AppUser> userManager;

		public IdentityClaimsProfileService(UserManager<AppUser> userManager, IUserClaimsPrincipalFactory<AppUser> claimsFactory)
		{
			this.userManager = userManager;
			this.claimsFactory = claimsFactory;
		}

		public async Task GetProfileDataAsync(ProfileDataRequestContext context)
		{
			var sub = context.Subject.GetSubjectId();
			var user = await userManager.FindByIdAsync(sub);
			var principal = await claimsFactory.CreateAsync(user);
			var roles = await userManager.GetRolesAsync(user);
			var claims = principal.Claims.ToList();
			claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
			claims.Add(new Claim(JwtClaimTypes.PreferredUserName, user.UserName));
			claims.Add(new Claim(IdentityServerConstants.StandardScopes.Email, user.Email));
			foreach (string role in roles) claims.Add(new Claim(JwtClaimTypes.Role, role));

			context.IssuedClaims = claims;
		}

		public async Task IsActiveAsync(IsActiveContext context)
		{
			var sub = context.Subject.GetSubjectId();
			var user = await userManager.FindByIdAsync(sub);
			context.IsActive = user != null;
		}
	}
}