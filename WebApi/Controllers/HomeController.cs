using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var filepath = Request.QueryString["path"];
            if(filepath != null)
            {
                return Open(filepath);
            }
            ViewBag.Title = "Home Page";
            return View(new Models.Directory());
        }

        public ActionResult Open(string path)
        {
            var file = System.IO.File.ReadAllBytes(path);
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = path,
                Inline = true
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(file, MimeMapping.GetMimeMapping(path));
        }
    }
}
