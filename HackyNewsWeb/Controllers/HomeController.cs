using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using HackyNewsDomain;

namespace HackyNewsWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var settings = new Data.Settings();
            var feed = new Models.Feed(settings);

            return View(feed);
        }
    }
}
