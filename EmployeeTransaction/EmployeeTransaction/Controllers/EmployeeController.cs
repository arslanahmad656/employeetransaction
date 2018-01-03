using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using EmployeeTransaction.Models;
using Microsoft.AspNet.Identity;

namespace EmployeeTransaction.Controllers
{
    [Authorize(Roles = "employee")]
    public class EmployeeController : Controller
    {
        private Entities db = new Entities();
        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: Employee
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var transactions = db.TransactionUsers.Where(t => t.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).Select(t => t.Transaction.code).ToList();
            return RedirectToAction(transactions.Contains(1) ? "EditCompany" : "CompanyDetails");
        }

        public ActionResult CompanyDetails()
        {
            return View(db.Companies.First());
        }

        public ActionResult EditCompany()
        {
            var userId = User.Identity.GetUserId();
            var transactions = db.TransactionUsers.Where(t => t.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).Select(t => t.Transaction.code).ToList();
            ViewBag.CanCreate = transactions.Contains(2);
            return View(db.Companies.First());
        }

        [HttpPost]
        public ActionResult EditCompany(Company model)
        {
            if(ModelState.IsValid)
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Please fill in the fields properly");
                return View(model);
            }
        }

        public ActionResult TransactionList()
        {
            var userId = User.Identity.GetUserId();
            var transactions = db.TransactionUsers.Where(t => t.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).Select(t => t.Transaction).ToList();
            return View(transactions);
        }

        public ActionResult FileList()
        {
            var userId = User.Identity.GetUserId();
            var transactions = db.TransactionUsers.Where(t => t.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).Select(t => t.Transaction.code).ToList();
            ViewBag.CanCreate = transactions.Contains(2);
            return View(db.Files.ToList());
        }

        public ActionResult CreateFiles()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveFiles(IEnumerable<HttpPostedFileBase> files)
        {
            foreach (var file in files)
            {
                if(file != null && file.ContentLength > 0)
                {
                    var imgName = Guid.NewGuid();
                    var serverPath = Server.MapPath("~/App_Data/Files");
                    var fullPath = Path.Combine(serverPath, imgName + Path.GetExtension(file.FileName));
                    file.SaveAs(fullPath);
                    db.Files.Add(new Models.File
                    {
                        Name = file.FileName
                    });
                    db.SaveChanges();
                }
            }
            return RedirectToAction("FileList");
        }
    }
}