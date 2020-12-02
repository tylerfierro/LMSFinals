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
    public class LessonsController : Controller
    {
        private LMSProjectEntities db = new LMSProjectEntities();

        // GET: Lessons
        public ActionResult Index()
        {
            var lesson = db.Lessons;

            //string currentUserID = User.Identity.GetUserId();
            //if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            //{
            //    return View(lessonViews.ToList());
            //}
            //else if (User.IsInRole("Employee"))
            //{
            //    var employeeViews = db.LessonViews.Where(x => x.UserId == currentUserID).Include(a => a.UserDetail);
            //    return View(employeeViews.ToList());
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            return View(lesson.ToList());
        }

        // GET: Lessons/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            string currentUserID = User.Identity.GetUserId();
            ViewBag.CompleteMessage = "You've completed this lesson! Good Job!";
            string body = $"{currentUserID} has completed the following lesson: a lesson";
            MailMessage m = new MailMessage("no-reply@tylerfierro.net", "tmfierro@outlook.com");
            m.IsBodyHtml = true;
            m.Priority = MailPriority.High;
            SmtpClient client = new SmtpClient("mail.tylerfierro.net");
            client.Credentials = new NetworkCredential("no-reply@tylerfierro.net", "Grapes123!");
            client.Port = 8889;
            client.Send(m);

            LessonView l = new LessonView();
            l.LessonId = (int)id;
            l.UserId = currentUserID;
            l.DateViewed = DateTime.Now;
            db.LessonViews.Add(l);
            db.SaveChanges();

            var nbrLessons = db.Lessons.Where(x => x.CourseId == lesson.CourseId).Count();
            var nbrLvs = db.LessonViews.Where(x => x.Lesson.CourseId == lesson.CourseId && x.UserId == currentUserID).Count();

            if (nbrLessons == nbrLvs)
            {
                CourseCompletion cc = new CourseCompletion();
                cc.CourseId = lesson.CourseId;
                cc.UserId = currentUserID;
                cc.DateCompleted = DateTime.Now;
                db.CourseCompletions.Add(cc);
                db.SaveChanges();
            }

            var v = lesson.Video.IndexOf("v=");
            var amp = lesson.Video.IndexOf("&", v);
            string vid;
            // if the video id is the last value in the url
            if (amp == -1)
            {
                vid = lesson.Video.Substring(v + 2);
                // if there are other parameters after the video id in the url
            }
            else
            {
                vid = lesson.Video.Substring(v + 2, amp - (v + 2));
            }
            ViewBag.VideoID = vid;
            return View(lesson);
        }

        // GET: Lessons/Create
        [Authorize(Roles = "Admin , Manager")]
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName");
            return View();
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin , Manager")]
        public ActionResult Create([Bind(Include = "LessonId,LessonName,CourseId,Intro,Video,PdfFilename,IsActive")] Lesson lesson, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                #region File Upload
                string imgName = "noImage.png";
                if (ImageFile != null)
                {
                    imgName = ImageFile.FileName;
                    string ext = imgName.Substring(imgName.LastIndexOf('.'));
                    string[] goodExts = { ".jpeg", ".jpg", ".gif", ".png" };
                    if (goodExts.Contains(ext.ToLower()) && (ImageFile.ContentLength <= 4194304))
                    {
                        imgName = Guid.NewGuid() + ext;
                        ImageFile.SaveAs(Server.MapPath("~/Content/assets/img/" + imgName));
                    }
                    else
                    {
                        imgName = "noImage.png";
                    }
                }
                lesson.PdfFilename = imgName;
                #endregion

                db.Lessons.Add(lesson);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", lesson.CourseId);
            return View(lesson);
        }

        // GET: Lessons/Edit/5
        [Authorize(Roles = "Admin , Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", lesson.CourseId);
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin , Manager")]
        public ActionResult Edit([Bind(Include = "LessonId,LessonName,CourseId,Intro,Video,PdfFilename,IsActive")] Lesson lesson, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                #region File Upload
                if (ImageFile != null)
                {
                    string imgName = ImageFile.FileName;
                    string ext = imgName.Substring(imgName.LastIndexOf('.'));
                    string[] goodExts = { ".jpeg", ",jpg", ".gif", ".png" };
                    if (goodExts.Contains(ext.ToLower()) && (ImageFile.ContentLength <= 4194304))
                    {
                        imgName = Guid.NewGuid() + ext;
                        ImageFile.SaveAs(Server.MapPath("~/Content/assets/img/" + imgName));
                        if(lesson.PdfFilename != null && lesson.PdfFilename != "noImage.png")
                        {
                            System.IO.File.Delete(Server.MapPath("~/Content/assets/img" + Session["currentImage"].ToString()));
                        }
                        lesson.PdfFilename = imgName;
                    }
                }
                #endregion
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseName", lesson.CourseId);
            return View(lesson);
        }

        // GET: Lessons/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Lesson lesson = db.Lessons.Find(id);
            if(lesson.PdfFilename != null && lesson.PdfFilename != "noImage.png")
            {
                System.IO.File.Delete(Server.MapPath("~/Content/assets/img/" + Session["currentImage"].ToString()));
            }
            db.Lessons.Remove(lesson);
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
