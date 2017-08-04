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
		private const string CONFIG_PATH = "config.yml";

		public ActionResult Index()
		{
			var configPath = Server.MapPath(CONFIG_PATH);

			var settings = new Data.Settings();
			settings.Load(configPath);
			var feed = new Models.Feed(settings);

			var maybeItems = feed.GetItems();
			return View(feed);
		}
	}
}
