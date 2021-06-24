using Identity_Service.Data;
using Identity_Service.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity_Service.Logic
{
	public class UserHandler
	{
		private readonly IServiceProvider serviceProvider;

		public UserHandler(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public async void DeleteUserById(string guid)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
				try
				{
					var user = await userManager.FindByIdAsync(guid);
					if (user == null)
					{
						Console.WriteLine("No User Found!");
						return;
					}
					await userManager.DeleteAsync(user);
					
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}
	}
}
