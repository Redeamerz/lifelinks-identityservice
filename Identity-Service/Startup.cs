using Identity_Service.Data;
using Identity_Service.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Identity_Service
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseMySql(Configuration.GetConnectionString("Default"), new MySqlServerVersion(new Version(8, 0, 21)));
			});

			services.AddIdentity<AppUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			services.AddTransient<IProfileService, IdentityClaimsProfileService>();

			var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

			// configure identity server with in-memory stores, keys, clients and scopes
			services.AddIdentityServer()
				.AddDeveloperSigningCredential()
				.AddAspNetIdentity<AppUser>()
				

			.AddConfigurationStore(options =>
			{
				options.ConfigureDbContext = builder =>

					builder.UseMySql(Configuration.GetConnectionString("Default"), new MySqlServerVersion(new Version(8,0,21)),
						mySql => mySql.MigrationsAssembly(migrationAssembly));

			})
			.AddOperationalStore(options =>
			{
				options.ConfigureDbContext = builder =>
				builder.UseMySql(Configuration.GetConnectionString("Default"), new MySqlServerVersion(new Version(8, 0, 21)),
				mySql => mySql.MigrationsAssembly(migrationAssembly));

				// token cleanup
				options.EnableTokenCleanup = true;
				options.TokenCleanupInterval = 30;
			});

			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie();

			services.AddCors();


			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity_Service", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			// seed database with start data if empty
			InitializeDatabase(app);
			var log = new LoggerConfiguration()
			.Enrich.FromLogContext()
			.WriteTo.File(new JsonFormatter(), "log.json")
			.CreateLogger();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity_Service v1"));
			}

			app.UseCors(options =>
			{
				options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
			});

			app.UseIdentityServer();

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
		private void InitializeDatabase(IApplicationBuilder app)
		{
			using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

				var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				context.Database.Migrate();
				if (!context.Clients.Any())
				{
					foreach (var client in Config.GetClients())
					{
						context.Clients.Add(client.ToEntity());
					}
					context.SaveChanges();
				}

				if (!context.IdentityResources.Any())
				{
					foreach (var resource in Config.GetIdentityResources())
					{
						context.IdentityResources.Add(resource.ToEntity());
					}
					context.SaveChanges();
				}

				if (!context.ApiResources.Any())
				{
					foreach (var resource in Config.GetApiResources())
					{
						context.ApiResources.Add(resource.ToEntity());
					}
					context.SaveChanges();
				}
			}
		}
	}
}
