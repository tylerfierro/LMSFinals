using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using LMSFinals.UI.MVC.Models;
using System.Threading.Tasks;
using System.IO;
using System;

namespace LMSFinals.UI.MVC.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string body = $"{model.Name} has sent you the following message: <br />" + $"{model.Message} <strong>from the email address:</strong> {model.Email}";
            MailMessage m = new MailMessage("no-reply@tylerfierro.net", "tmfierro@outlook.com", model.Subject, body);
            m.IsBodyHtml = true;
            m.Priority = MailPriority.High;
            m.ReplyToList.Add(model.Email);
            SmtpClient client = new SmtpClient("mail.tylerfierro.net");
            client.Credentials = new NetworkCredential("no-reply@tylerfierro.net", "Grapes123!");
            client.Port = 8889;
            try
            {
                client.Send(m);
            }
            catch (Exception e)
            {
                ViewBag.Message = e.StackTrace;
            }
            return View("EmailConfirmation", model);
        }

        public ActionResult Sent()
        {
            return View();
        }
    }
}
