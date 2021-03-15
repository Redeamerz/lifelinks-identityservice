using System;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity_Service
{
	public class Config
	{
		public static IEnumerable<IdentityResource> GetIdentityResources()
		{
			return new List<IdentityResource>
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Email(),
				new IdentityResources.Profile()
			};
		}

		public static IEnumerable<ApiResource> GetApiResources()
		{
			return new List<ApiResource>
			{
				new ApiResource("apigateway", "Gateway to lifelinks apis")
			};
		}

		public static IEnumerable<Client> GetClients()
		{
			// client credentials client
			return new List<Client>
			{
				// resource owner password grant
				new Client
				{
					ClientId = "ro.vue",
					AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

					ClientSecrets =
					{
						new Secret("".Sha256())
					},
					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						IdentityServerConstants.StandardScopes.Email,
						IdentityServerConstants.StandardScopes.Address,
						"apigateway"
					}
				}
			};
		}
	}
}
