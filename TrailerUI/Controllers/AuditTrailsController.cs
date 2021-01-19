using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrailerUI.Models;

namespace TrailerUI.Controllers
{
    public class AuditTrailsController : Controller
    {

        private AppDbContext db = new AppDbContext();

        // GET: AuditTrails
        public ActionResult Index()
        {
            return View(db.AuditTrails.OrderByDescending(p => p.Id).Take(50).ToList());
        }

        [HttpPost]
        public ActionResult Index(DateTime Start, DateTime End)
        {
            return View(db.AuditTrails.OrderByDescending(p => p.Id).Where(p => p.AuditDateTime >= Start && p.AuditDateTime <= End).Take(50).ToList());
        }
    }
}
