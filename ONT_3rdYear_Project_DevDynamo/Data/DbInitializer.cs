using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using ONT_3rdyear_Project.Models;

namespace ONT_3rdyear_Project.Data
{
	public static class DbInitializer
	{
		public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
		{
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
			var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

			string[] roles = { "Admin", "Doctor", "Nurse","Sister", "WardAdmin", "ScriptManager", "ConsumableManager" };

			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
					await roleManager.CreateAsync(new IdentityRole<int>(role));
			}

			// Seed the Admin user
			var adminEmail = "admin@hospital.com";
			var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
			if (existingAdmin == null)
			{
				var adminUser = new ApplicationUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					FullName = "System Admin",
					RoleType = "Admin",
					EmailConfirmed = true,
					SecurityStamp = Guid.NewGuid().ToString() 
				};

				var result = await userManager.CreateAsync(adminUser, "Admin@123");

				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(adminUser, "Admin");
				}
			}
		}
	}
}
