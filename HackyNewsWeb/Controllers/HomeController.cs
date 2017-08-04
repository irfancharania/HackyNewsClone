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

            var maybeItems = feed.GetItems();
            if(maybeItems.Success()) {
                foreach(var result in maybeItems.GetResult()) {
                    if(result.Success()){
                        var item = result.GetResult().item;
                        Console.WriteLine(item.title);
                    } else {
                        var item = result.GetError().Item1;
                        var message = result.GetError().Item2;

						Console.WriteLine(item.title);
						Console.WriteLine(message);
                    }
                }
            }


            return View(feed);
        }
    }
}
