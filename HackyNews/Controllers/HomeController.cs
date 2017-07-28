using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HackyNewsDomain;

namespace HackyNews.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var settings = new Data.Settings();
            var data = Data.getData(settings);
            return View(data);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
