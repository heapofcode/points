using app.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;

namespace app.Model
{
    public class appInitContext
    {
        public async static void initContextDB(AppDbContext _context, UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            await _context.Database.EnsureCreatedAsync();

            if (!_context.UserRoles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.admin));
                await _roleManager.CreateAsync(new IdentityRole(Roles.user));
            }

            if (!_context.Users.Any())
            {
                var Admin = new ApplicationUser() { Email = "admin@mail.com", UserName = "admin",  EmailConfirmed = true };
                var isAdmin = await _userManager.CreateAsync(Admin, "123456789");
                if (isAdmin.Succeeded)
                {
                    await _userManager.AddToRolesAsync(Admin, new List<string>{ Roles.admin, Roles.user });
                }

                var User = new ApplicationUser() { Email = "user@mail.com", UserName = "user", EmailConfirmed = true };
                var isUser = await _userManager.CreateAsync(User, "123456789");
                if (isUser.Succeeded)
                {
                    await _userManager.AddToRolesAsync(User, new List<string>{ Roles.user });
                }
            }

            if (!_context.PetitionTypes.Any()) 
            {
                await _context.PetitionTypes.AddRangeAsync(
                    new List<PetitionType>
                    {
                        new PetitionType { Title = "Постоянный"},
                        new PetitionType { Title = "Временный"},
                    }
                );
            }    

            await _context.SaveChangesAsync();
        }
    }
}