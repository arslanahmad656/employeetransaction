using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using EmployeeTransaction.Models;
using System.Web.Optimization;
using System;

[assembly: OwinStartupAttribute(typeof(EmployeeTransaction.Startup))]
namespace EmployeeTransaction
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            AddDefaultRolesAndUsers();
        }

        public void AddDefaultRolesAndUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            Entities db = new Entities();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var adminRole = "admin";
            var managerRole = "manager";
            var employeeRole = "employee";

            // Creating the admin role
            if(!roleManager.RoleExists(adminRole))
            {
                var roleName = adminRole;
                var role = new IdentityRole
                {
                    Name = roleName
                };
                var result = roleManager.Create(role);
                if(result.Succeeded)
                {
                    var user = new ApplicationUser
                    {
                        Email = "admin@gmail.com",
                        UserName = "admin"
                    };
                    var password = "updating";
                    result = UserManager.Create(user, password);
                    if(result.Succeeded)
                    {
                        result = UserManager.AddToRole(user.Id, roleName);
                        if(!result.Succeeded)
                        {
                            throw new Exception("Could not add user admin to role admin");
                        }
                    }
                    else
                    {
                        throw new Exception("Could not create user admin");
                    }
                }
                else
                {
                    throw new Exception("Could not create role admin");
                }
            }

            // Creating Manager Role
            if(!roleManager.RoleExists(managerRole))
            {
                var roleName = managerRole;
                var role = new IdentityRole
                {
                    Name = roleName
                };
                var result = roleManager.Create(role);
                if(!result.Succeeded)
                {
                    throw new Exception("Could not create role manager");
                }
            }

            // Creating Employee Role
            if (!roleManager.RoleExists(employeeRole))
            {
                var roleName = employeeRole;
                var role = new IdentityRole
                {
                    Name = roleName
                };
                var result = roleManager.Create(role);
                if (!result.Succeeded)
                {
                    throw new Exception("Could not create role employee");
                }

                // dummy company
                db.Companies.Add(new Company
                {
                    CompanyName = "Company Name",
                    Date1 = DateTime.Now,
                    Date2 = DateTime.Now.Subtract(new TimeSpan(200, 0, 0, 0, 0)),
                    Financial = "My finance",
                    Name = "Name",
                    Proposal = "Proposal",
                    State = "State",
                    Status = "Editable",
                    ToWhom = "Someone",
                    Words = "word"
                });

                // Transactions
                db.Transactions.Add(new Transaction
                {
                    code = 1,
                    Name = "Edit"
                });

                db.Transactions.Add(new Transaction
                {
                    code = 2,
                    Name = "Create"
                });

                db.SaveChanges();
            }
        }
    }
}
