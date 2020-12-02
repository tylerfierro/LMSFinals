using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using LMSFinals.DATA.EF;
using Microsoft.AspNet.Identity;

namespace LMSFinals.UI.MVC.Controllers
{
    public class LessonViewsController : Controller
    {
        private LMSProjectEntities db = new LMSProjectEntities();

        // GET: LessonViews
        public ActionResult Index()
        {
            var lessonViews = db.LessonViews.Include(l => l.Lesson).Include(l => l.UserDetail);

            string currentUserID = User.Identity.GetUserId();            if (User.IsInRole("Admin") || User.IsInRole("Manager"))            {                return View(lessonViews.ToList());            }            else if (User.IsInRole("Employee"))            {                var employeeViews = db.LessonViews.Where(x => x.UserId == currentUserID).Include(a => a.UserDetail);                return View(employeeViews.ToList());            }            else            {
                return RedirectToAction("Index", "Home");            }
        }

        // GET: LessonViews/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LessonView lessonView = db.LessonViews.Find(id);
            if (lessonView == null)
            {
                return HttpNotFound();
            }
            return View(lessonView);
        }

        // Post: LessonViews/Completion
        public ActionResult CompletionLesson()
        {
            // if (if the lesson is completed then I want the submit button from the user to send me an email to tell me what lesson was completed and who completed it.) 
            int lessonCount = 0;
            string currentUserID = User.Identity.GetUserId();
            //var currentLesson = this.db.Lessons.LessonId
            if (lessonCount < 1)
            {
                lessonCount++;
                ViewBag.CompleteMessage = "You've completed this lesson! Good Job!";
                string body = $"{currentUserID} has completed the following lesson: a lesson";
                MailMessage m = new MailMessage("no-reply@tylerfierro.net", "tmfierro@outlook.com");
                m.IsBodyHtml = true;
                m.Priority = MailPriority.High;
                SmtpClient client = new SmtpClient("mail.tylerfierro.net");
                client.Credentials = new NetworkCredential("no-reply@tylerfierro.net", "Grapes123!");
                client.Port = 8889;
            }
            return View(currentUserID.ToList());
        }


        // GET: LessonViews/Create
        [Authorize(Roles = "Admin , Manager")]
        public ActionResult Create()
        {
            ViewBag.LessonId = new SelectList(db.Lessons, "LessonId", "LessonName");
            ViewBag.UserId = new SelectList(db.UserDetails, "UserId", "FirstName");
            return View();
        }

        // POST: LessonViews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin , Manager")]
        public ActionResult Create([Bind(Include = "LessonViewId,UserId,LessonId,DateViewed")] LessonView lessonView)
        {
            if (ModelState.IsValid)
            {
                db.LessonViews.Add(lessonView);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LessonId = new SelectList(db.Lessons, "LessonId", "LessonName", lessonView.LessonId);
            ViewBag.UserId = new SelectList(db.UserDetails, "UserId", "FirstName", lessonView.UserId);
            return View(lessonView);
        }

        // GET: LessonViews/Edit/5
        [Authorize(Roles = "Admin , Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LessonView lessonView = db.LessonViews.Find(id);
            if (lessonView == null)
            {
                return HttpNotFound();
            }
            ViewBag.LessonId = new SelectList(db.Lessons, "LessonId", "LessonName", lessonView.LessonId);
            ViewBag.UserId = new SelectList(db.UserDetails, "UserId", "FirstName", lessonView.UserId);
            return View(lessonView);
        }

        // POST: LessonViews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin , Manager")]
        public ActionResult Edit([Bind(Include = "LessonViewId,UserId,LessonId,DateViewed")] LessonView lessonView)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lessonView).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LessonId = new SelectList(db.Lessons, "LessonId", "LessonName", lessonView.LessonId);
            ViewBag.UserId = new SelectList(db.UserDetails, "UserId", "FirstName", lessonView.UserId);
            return View(lessonView);
        }

        // GET: LessonViews/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LessonView lessonView = db.LessonViews.Find(id);
            if (lessonView == null)
            {
                return HttpNotFound();
            }
            return View(lessonView);
        }

        // POST: LessonViews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            LessonView lessonView = db.LessonViews.Find(id);
            db.LessonViews.Remove(lessonView);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
