using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataAccessLayer;
using Microsoft.AspNet.Identity;
using DataAccessLayer.Models;
using System.Net.Mail;

namespace WebAPI.Controllers
{
    public class APIHomeController : ApiController
    {
        private JobFinderDbContext db = new JobFinderDbContext();


        [HttpGet]
        [Route("api/APIHome/Index")]
        public IHttpActionResult Index()
        {
            try
            {
                //db.Configuration.ProxyCreationEnabled = false;
                
                var categories = db.Categories.ToList();
                var CurrentUserPostedJobsId = Enumerable.Empty<int>();
                var CurrentUserAppliedJobsId = Enumerable.Empty<int>();

                if (User.Identity.IsAuthenticated && User.IsInRole("Publisher"))
                {
                    var CurrentUserId = User.Identity.GetUserId();
                    CurrentUserPostedJobsId = db.Jobs.Where(x => x.PublisherId == CurrentUserId).Select(x => x.Id).ToList();
                }
                else if(User.Identity.IsAuthenticated && User.IsInRole("Applicant"))
                {
                    var CurrentUserId = User.Identity.GetUserId();
                    CurrentUserAppliedJobsId = db.Jobs.Where(x => x.Applies.Any(y => y.ApplicantId == CurrentUserId)).Select(x => x.Id).ToList();
                }

                return Ok(new { categories, CurrentUserPostedJobsId, CurrentUserAppliedJobsId });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/APIHome/Search")]
        public IHttpActionResult Search(string SearchJob)
        {
            try
            {
                var jobs = db.Jobs.Where(x => x.IsSuspended == false && (x.JobTitle.Contains(SearchJob) || x.JobContent.Contains(SearchJob)
                                       || x.Category.CategoryName.Contains(SearchJob) || x.Category.CategoryDescription.Contains(SearchJob))).ToList();

                var CurrentUserPostedJobsId = Enumerable.Empty<int>();
                var CurrentUserAppliedJobsId = Enumerable.Empty<int>();

                if (User.Identity.IsAuthenticated && User.IsInRole("Publisher"))
                {
                    var CurrentUserId = User.Identity.GetUserId();
                    CurrentUserPostedJobsId = db.Jobs.Where(x => x.PublisherId == CurrentUserId).Select(x => x.Id).ToList();
                }
                else if (User.Identity.IsAuthenticated && User.IsInRole("Applicant"))
                {
                    var CurrentUserId = User.Identity.GetUserId();
                    CurrentUserAppliedJobsId = db.Jobs.Where(x => x.Applies.Any(y => y.ApplicantId == CurrentUserId)).Select(x => x.Id).ToList();
                }

                return Ok(new { jobs, CurrentUserPostedJobsId, CurrentUserAppliedJobsId });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }


        [HttpPost]
        [Route("api/APIHome/Contact")]
        public IHttpActionResult Contact(ContactViewModel contact)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = db.AspNetUsers.Find(User.Identity.GetUserId());
                    string fromEmail = "anonymousUser@gmail.com";
                    string userName = "anonymousUser";
                    if (user != null)
                    {
                        fromEmail = user.Email;
                        userName = user.UserName;
                    }

                    MailMessage mail = new MailMessage(fromEmail, "applicationjobfinder@gmail.com");
                    mail.Body = "Name : " + userName + "<br>"
                                + "Email : " + fromEmail + "<br>"
                                + "Subject : " + contact.Subject + "<br>"
                                + "Message : <b>" + contact.Message + "<b/>";
                    mail.Subject = contact.Subject;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;

                    // these credentials is used to send the email to the receiepent (Mail.To)                    
                    smtp.Credentials = new System.Net.NetworkCredential()
                    {
                        UserName = "applicationjobfinder@gmail.com",
                        Password = "123456@Sd"
                    };
                    smtp.Send(mail);

                    var status = "The Message is successfully sent. you will be redirected to Home page in 5seconds.";
                    return Ok(status);
                }

                return BadRequest(ModelState);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/APIHome/PublisherPostedJobs")]
        public IHttpActionResult PublisherPostedJobs(string publisherName)
        {
            try
            {
                var jobs = db.Jobs.Where(x => x.IsSuspended == false && x.Publisher.UserName == publisherName).ToList();
                var CurrentUserAppliedJobsId = Enumerable.Empty<int>();

                if (User.Identity.IsAuthenticated && User.IsInRole("Applicant"))
                {
                    var CurrentUserId = User.Identity.GetUserId();
                    CurrentUserAppliedJobsId = jobs.Where(x => x.Applies.Any(y => y.ApplicantId == CurrentUserId)).Select(x => x.Id).ToList();
                }

                return Ok(new { jobs, CurrentUserAppliedJobsId });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/APIHome/NotificationData")]
        public IHttpActionResult NotificationData()
        {
            try
            {
                var CurrentUser = db.AspNetUsers.Find(User.Identity.GetUserId());

                var TotalNots = db.Notifications.Where(x => x.ReceiverEmail == CurrentUser.Email).Count();
                var NewNots = db.Notifications.Where(x => x.ReceiverEmail == CurrentUser.Email && x.IsRead == false).Count();
                var LastNewNot = db.Notifications.Where(x => x.ReceiverEmail == CurrentUser.Email && x.IsRead == false).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                // https://docs.microsoft.com/en-us/dotnet/api/system.timespan?view=netframework-4.8
                var LastNewNotTime = LastNewNot == null ? "0 mins" : TimeOfLastNot(DateTime.Now.Subtract(LastNewNot.CreatedDate));

                return Ok(new { TotalNots, NewNots, LastNewNotTime });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string TimeOfLastNot(TimeSpan t)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.timespan?view=netframework-4.8
            if (t.Days > 0)
                return t.Days.ToString() + " days";
            else if (t.Hours > 0)
                return t.Hours.ToString() + " hours";
            else if (t.Minutes > 0)
                return t.Minutes.ToString() + " mins";
            else
            {
                return "0 secs";
            }
        }


        [HttpGet]
        [Authorize(Roles = "Publisher, Applicant")]
        [Route("api/APIHome/GetNotifications")]
        public IHttpActionResult GetNotifications(string status)
        {
            try
            {
                var CurrentUser = db.AspNetUsers.Find(User.Identity.GetUserId());
                List<Notification> nots;
                bool NewNot = false;

                if (status == "New")
                {
                    nots = db.Notifications.Where(x => x.ReceiverEmail == CurrentUser.Email && x.IsRead == false).OrderByDescending(x => x.CreatedDate).ToList();
                    NewNot = true;
                }
                else
                {
                    nots = db.Notifications.Where(x => x.ReceiverEmail == CurrentUser.Email).OrderByDescending(x => x.CreatedDate).ToList();
                }

                return Ok(new { nots, NewNot });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Publisher, Applicant")]
        [Route("api/APIHome/OpenNotification/{id}")]
        public IHttpActionResult OpenNotification(int id)
        {
            try
            {
                var CurrentUser = db.AspNetUsers.Find(User.Identity.GetUserId());
                var not = db.Notifications.Where(x => x.Id == id && x.ReceiverEmail == CurrentUser.Email).FirstOrDefault();

                if (not == null)
                {
                    return NotFound();
                }

                not.IsRead = true;
                db.SaveChanges();

                return Ok(not);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Publisher, Applicant")]
        [Route("api/APIHome/MarkNotAsRead/{id}")]
        public IHttpActionResult MarkNotAsRead(int id)
        {
            try
            {
                var CurrentUser = db.AspNetUsers.Find(User.Identity.GetUserId());
                var not = db.Notifications.Where(x => x.Id == id && x.ReceiverEmail == CurrentUser.Email).FirstOrDefault();

                if (not == null)
                {
                    return NotFound();
                }

                not.IsRead = true;
                db.SaveChanges();

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);      
            }
        }        


        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
