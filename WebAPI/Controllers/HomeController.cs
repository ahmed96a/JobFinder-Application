using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccessLayer;

namespace WebAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            using (JobFinderDbContext db = new JobFinderDbContext())
            {
                ViewBag.UserType = new SelectList(db.AspNetRoles.Where(x => x.Name != "SuperAdmin" && x.Name != "Admin").ToList(), "Name", "Name");
            }                
            return View();
        }

        
    }
}
