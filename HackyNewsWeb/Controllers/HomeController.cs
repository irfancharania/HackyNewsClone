using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using HackyNewsDomain;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HackyNewsWeb.Controllers
{
	public class HomeController : Controller
	{
		private const string CONFIG_PATH = "~/config.yml";

		public ActionResult Index()
		{
			// Create new stopwatch.
			//Stopwatch stopwatch = new Stopwatch();

			// Begin timing.
			//stopwatch.Start();

			Log.Logger = new LoggerConfiguration()
        	  .Enrich.FromLogContext()
        	  .WriteTo.LiterateConsole()
        	  .CreateLogger();
            
            ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog();
            var logger = loggerFactory.CreateLogger<HomeController>();

			var configPath = Server.MapPath(CONFIG_PATH);

			var settings = new Data.Settings();
			settings.Load(configPath);

			var feed = new Models.Feed(settings, logger);
			var model = feed.GetItems(); 
			

			//var output = result.GetResult().ToList();
			// Stop timing.
			//stopwatch.Stop();

			return View(model);
		}
	}
}
