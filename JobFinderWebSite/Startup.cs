using JobFinderWebSite.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JobFinderWebSite.Startup))]
namespace JobFinderWebSite
{
    public partial class Startup
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Call CreateDefaultRolesAndUsers
            CreateDefaultRolesAndUsers(); // Part 22
        }

        // Part 22
        //------------------------------------------------

        // Create Admin role and add user that belong to that role.
        public void CreateDefaultRolesAndUsers()
        {
            // RoleManager  is used to manage roles.
            // RoleStore    is used to store roles in the database.
            // IdentityRole is the model class of the AspNetRoles table.

            // UserManager  is used to manage users.
            // UserStore    is used to store users in the database.
            // ApplicationUser is the model class of the AspNetUsers table. (which inherts from IdentityUser class)

            // create instance from RoleManager that manage the roles and pass instance of RoleStore class, so we can store roles in the database.
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            // create instance from UserManager that manage the users and pass instance of UserStore class, so we can store users in the database.
            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            // we write that uservalidator, so we can use space in the username "Asd Asd"
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Add Admin role if not exists then create and add user to it.
            if (!roleManager.RoleExists("SuperAdmin"))
            {
                IdentityRole role = new IdentityRole("SuperAdmin");
                roleManager.Create(role);

                ApplicationUser user = new ApplicationUser();

                user.UserName = "Asd Asd";
                user.Email = "asd@gmail.com";
                user.UserType = "SuperAdmin";

                var check = userManager.Create(user, "123456@Sd");

                if (check.Succeeded)
                {
                    userManager.AddToRole(user.Id, "SuperAdmin");
                }
            }

            // Add Admin role
            if (!roleManager.RoleExists("Admin"))
            {
                IdentityRole role = new IdentityRole("Admin");
                roleManager.Create(role);
            }

            // Add Publisher role
            if (!roleManager.RoleExists("Publisher"))
            {
                IdentityRole role = new IdentityRole("Publisher");
                roleManager.Create(role);
            }

            // Add Applicant role
            if (!roleManager.RoleExists("Applicant"))
            {
                IdentityRole role = new IdentityRole("Applicant");
                roleManager.Create(role);
            }

        }
        //------------------------------------------------

    }
}
