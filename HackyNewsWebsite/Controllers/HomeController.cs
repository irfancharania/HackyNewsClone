using HackyNewsDomain;
using HackyNewsWebsite.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace HackyNewsWebsite.Controllers
{
    public class HomeController : Controller
    {
        private const string _configFile = "config.yml";
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        public HomeController(IHostingEnvironment hostingEnvironment,
                              ILogger<HomeController> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("We're on the home page!");

            var configPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configFile);

            var settings = new Data.Settings();
            settings.Load(configPath);
            
            var feed = new HackyNewsDomain.Export.Feed(settings, _logger);
            var model = feed.GetItems();


            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
