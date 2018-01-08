using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using EmployeeTransaction.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace EmployeeTransaction.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {

        private Entities db = new Entities();
        private ApplicationDbContext context = new ApplicationDbContext();
        // GET: Admin
        public ActionResult Index()
        {
            //return RedirectToAction("EmployeeList");
            return RedirectToAction("CompanyDetails");
        }

        public ActionResult CompanyDetails()
        {
            ViewBag.Role = "Admin";
            return View(db.Companies.ToList());
        }

        //public ActionResult EditCompany()
        //{
        //    ViewBag.Role = "Admin"; // Since it's in admin controller, it's always the admin role
        //    return View(db.Companies.First());
        //}

        //[HttpPost]
        //public ActionResult EditCompany(Company model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(model).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("", "Please fill in the fields properly");
        //        return View(model);
        //    }
        //}

        public ActionResult EmployeeList()
        {
            var currentUser = context.Users.Find(GetCurrentManager());
            ViewBag.CurrentManager = currentUser?.UserName ?? "Not Assigned Yet";
            var employeeRoleId = db.AspNetRoles.Where(role => role.Name.Equals("employee", StringComparison.OrdinalIgnoreCase)).Single().Id;
            var employees = context.Users.Where(user => user.Roles.Select(role => role.RoleId).Contains(employeeRoleId)).ToList();
            return View(employees);
        }

        public ActionResult EmployeeDetails(string id)
        {
            var transactions = db.TransactionUsers.Where(t => t.UserId.Equals(id, StringComparison.OrdinalIgnoreCase)).Select(t => t.Transaction).ToList();
            ViewBag.Transactions = transactions;
            return View(db.AspNetUsers.Find(id));
        }

        public ActionResult CreateEmployee()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateEmployee(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Username
                };
                var result = userManager.Create(user, model.Password);
                result = userManager.AddToRole(user.Id, "employee");
                return RedirectToAction("EmployeeList");
            }
            else
            {
                ModelState.AddModelError("", "Fill in the fields properly");
                return View(model);
            }
        }

        public ActionResult CreateManager(string id)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            IdentityResult result;
            try
            {
                result = userManager.RemoveFromRole(GetCurrentManager(), "manager");
            }
            catch
            {

            }
            result = userManager.AddToRole(id, "manager");
            return RedirectToAction("EmployeeList");
        }

        public ActionResult ManageTransactions(string id)
        {
            ViewBag.UserId = id;
            var transactions = db.TransactionUsers.Where(t => t.UserId.Equals(id, StringComparison.OrdinalIgnoreCase)).Select(t => t.Transaction.code).ToList();
            ViewBag.CanEdit = transactions.Contains(1);
            ViewBag.CanCreate = transactions.Contains(2);
            return View(db.Transactions.ToList());
        }

        [HttpPost]
        public ActionResult SaveTransactions()
        {
            var userId = Request.Form["user_id"];
            var transactions = db.TransactionUsers.Where(t => t.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).ToList();
            transactions.ForEach(t => db.Entry(t).State = System.Data.Entity.EntityState.Deleted);
            try
            {
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                var m = ex.Message;
            }
            var canCreate = Request.Form["can_create"].Split(',').Length == 2;
            var canEdit = Request.Form["can_edit"].Split(',').Length == 2;

            var canCreateId = db.Transactions.Where(t => t.code == 2).First().Id;
            var canEditId = db.Transactions.Where(t => t.code == 1).First().Id;
            if (canCreate)
            {
                db.TransactionUsers.Add(new TransactionUser
                {
                    TransactionId = canCreateId,
                    UserId = userId
                });
                db.SaveChanges();
            }
            if (canEdit)
            {
                db.TransactionUsers.Add(new TransactionUser
                {
                    TransactionId = canEditId,
                    UserId = userId
                });
                db.SaveChanges();
            }
            return RedirectToAction("EmployeeDetails", new { id = userId });
        }

        public ActionResult AddCompany()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddCompany(Company model)
        {
            if(ModelState.IsValid)
            {
                db.Companies.Add(model);
                db.SaveChanges();
                return RedirectToAction("CompanyDetails");
            }
            else
            {
                ModelState.AddModelError("", "Please fill in all the fields properly.");
                return View(model);
            }
        }

        public ActionResult ViewCompanyAssignment(int id)
        {
            var company = db.Companies.Find(id);
            if(company == null)
            {
                return new HttpNotFoundResult("Company with specified id not found");
            }
            var employees = db.EmployeeCompanies.Where(ec => ec.CompanyId == company.Id).Select(ec => ec.AspNetUser).ToList();
            ViewBag.CompanyName = company.CompanyName;
            ViewBag.CompanyId = company.Id;
            return View(employees);
        }

        public ActionResult EditCompanyAssignment(int id)
        {
            var company = db.Companies.Find(id);
            if(company == null)
            {
                return new HttpNotFoundResult("Company with specified id not found");
            }
            var employeesWithCompany = db.EmployeeCompanies.Where(ec => ec.CompanyId == company.Id).Select(ec => ec.AspNetUser.Id).ToList();

            var employeeRoleId = db.AspNetRoles.Where(role => role.Name.Equals("employee", StringComparison.OrdinalIgnoreCase)).Single().Id;
            var allEmployees = context.Users
                .Where(user => user.Roles
                    .Select(role => role.RoleId)
                    .Contains(employeeRoleId))
                .Select(e => new SelectListItem
                {
                    Text = e.UserName,
                    Value = e.Id,
                    Selected = employeesWithCompany.Contains(e.Id)
                })
                .ToList();

            ViewBag.EmployeesSelect = allEmployees;
            ViewBag.CompanyName = company.CompanyName;
            ViewBag.CompanyId = company.Id;
            return View();
        }

        [HttpPost]
        public ActionResult SaveCompanyAssignments()
        {
            var companyId = Convert.ToInt32(Request.Form["company_id"]);
            db.EmployeeCompanies
                .Where(ec => ec.CompanyId == companyId)
                .ToList()
                .ForEach(ec => db.Entry(ec).State = System.Data.Entity.EntityState.Deleted);
            db.SaveChanges();
            try
            {
                var newEmployees = Request.Form["employees"].Split(',').ToList();
                newEmployees.ForEach(e => db.EmployeeCompanies.Add(new EmployeeCompany
                {
                    CompanyId = companyId,
                    EmployeeId = e
                }));
                db.SaveChanges();
            }
            catch (NullReferenceException)
            {   
            }
            return RedirectToAction("ViewCompanyAssignment", new { id = companyId });
        }

        private string GetCurrentManager()
        {
            var managerRoleId = db.AspNetRoles.Where(r => r.Name.Equals("manager", StringComparison.OrdinalIgnoreCase)).First().Id;
            try
            {
                var managerId = context.Users.Where(user => user.Roles.Select(role => role.RoleId).Contains(managerRoleId)).First().Id;
                return managerId;
            }
            catch
            {
                return null;
            }
        }
    }
}