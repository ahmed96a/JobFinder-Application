using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataAccessLayer;
using Microsoft.AspNet.Identity;
using System.Web;
using System.IO;
using WebAPI.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Mail;
using System.Web.Http.ModelBinding.Binders;
using DataAccessLayer.Models;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AdminsController : ApiController
    {
        private JobFinderDbContext db = new JobFinderDbContext();
        private string CVPath;
        private string JobImagesPath;

        public AdminsController()
        {
            var WebAPIPath = HttpContext.Current.Server.MapPath("~");
            var SolutionPathIndex = WebAPIPath.LastIndexOf("WebAPI");
            var SolutionPath = WebAPIPath.Substring(0, SolutionPathIndex);

            CVPath = Path.Combine(SolutionPath, @"JobFinderWebSite\Uploads\CV\");
            JobImagesPath = Path.Combine(SolutionPath, @"JobFinderWebSite\Uploads\JobImages\");
        }

        [HttpGet]
        [Route("api/Admins/Index")]
        public IHttpActionResult Index()
        {
            try
            {
                var PublishersCount = db.AspNetUsers.Where(x => x.Roles.Any(y => y.Name == "Publisher")).Count();
                var ApplicantsCount = db.AspNetRoles.Where(x => x.Name == "Applicant").First().Users.Count();
                var AdminsCount     = db.AspNetUsers.Where(x => x.UserType == "Admin").Count();
                var JobsCount       = db.Jobs.Count();
                var ApplyForJobsCount    = db.ApplyForJobs.Count();
                var CategoriesCount = db.Categories.Count();

                return Ok(new { PublishersCount, ApplicantsCount, AdminsCount, JobsCount, ApplyForJobsCount, CategoriesCount });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Admins/GetAdmins")]
        public IHttpActionResult GetAdmins(string search)
        {
            try
            {
                List<AspNetUser> admins;                
                var CurrentAdminId = User.Identity.GetUserId();

                if (!string.IsNullOrEmpty(search))
                {
                    admins = db.AspNetUsers.Where(x => (x.Roles.Any(r => r.Name == "Admin" || r.Name == "SuperAdmin") && x.UserName.ToLower().Contains(search)) && x.Id != CurrentAdminId).ToList();
                }
                else
                {                    
                    admins = db.AspNetUsers.Where(x => (x.Roles.Any(r => r.Name == "Admin") || x.Roles.Any(r => r.Name == "SuperAdmin")) && x.Id != CurrentAdminId).ToList();
                }

                return Ok(admins);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }


        [HttpGet]
        [Route("api/Admins/GetPublishers")]
        public IHttpActionResult GetPublishers(string search)
        {
            try
            {
                List<AspNetUser> Publishers;

                if (!string.IsNullOrEmpty(search))
                {
                    Publishers = db.AspNetUsers.Where(x => x.Roles.Any(r => r.Name == "Publisher") && x.UserName.ToLower().Contains(search)).ToList();
                }
                else
                {
                    Publishers = db.AspNetUsers.Where(x => x.Roles.Any(r => r.Name == "Publisher")).ToList();
                }

                return Ok(Publishers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Admins/GetApplicants")]
        public IHttpActionResult GetApplicants(string search)
        {
            try
            {
                List<AspNetUser> Applicants;

                if (!string.IsNullOrEmpty(search))
                {
                    Applicants = db.AspNetUsers.Where(x => x.Roles.Any(r => r.Name == "Applicant") && x.UserName.ToLower().Contains(search)).ToList();
                }
                else
                {
                    Applicants = db.AspNetUsers.Where(x => x.Roles.Any(r => r.Name == "Applicant")).ToList();
                }

                return Ok(Applicants);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Admins/GetCategories")]
        public IHttpActionResult GetCategories(string search)
        {
            try
            {
                List<Category> Categories;

                if (!string.IsNullOrEmpty(search))
                {
                    Categories = db.Categories.Where(x => x.CategoryName.ToLower().Contains(search)).ToList();
                }
                else
                {
                    Categories = db.Categories.ToList();
                }

                return Ok(Categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Admins/GetJobs")]
        public IHttpActionResult GetJobs(int? CategoryId, int? JobStatus, string Publisher, string Search)
        {
            try
            {
                string category = "All";
                string jobstatus = "";

                var jobs = db.Jobs.ToList();

                if (!string.IsNullOrEmpty(Publisher))
                {
                    jobs = db.Jobs.Where(x => x.Publisher.UserName == Publisher).ToList();
                }

                if (!string.IsNullOrEmpty(Search))
                {
                    jobs = db.Jobs.Where(x => x.JobTitle.ToLower().Contains(Search.ToLower())).ToList();
                }

                if (CategoryId != null)
                {
                    jobs = jobs.Where(x => x.CategoryId == CategoryId).ToList();
                    category = db.Categories.Find(CategoryId).CategoryName;
                }

                if (JobStatus != null)
                {
                    jobs = jobs.Where(x => x.IsSuspended == Convert.ToBoolean(JobStatus)).ToList();
                    jobstatus = JobStatus == 1 ? " Suspended" : " Active";
                }

                var categories = db.Categories.ToList();   // new SelectList(db.Categories.ToList(), "Id", "CategoryName");
                var JobTitle = category + jobstatus + " Jobs";

                return Ok(new { categories, JobTitle, jobs });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }


        [HttpGet]
        [Route("api/Admins/GetApplies")]
        public IHttpActionResult GetApplies(int? CategoryId, int? JobStatus, string ApplyStatus, string Applicant, int? JobId, string search)
        {
            try
            {
                string apply = "All";
                string category = "All";
                string jobstatus = "";
                string JobIdTitle = string.Empty;
                var categories = db.Categories.ToList();

                var applies = db.ApplyForJobs.ToList();

                if (!string.IsNullOrEmpty(search))
                {
                    applies = applies.Where(x => x.Applicant.UserName.ToLower().Contains(search.ToLower())).ToList();
                }

                if (!string.IsNullOrEmpty(Applicant))
                {
                    applies = applies.Where(x => x.Applicant.UserName == Applicant).ToList();                    
                }

                if (JobId != null)
                {
                    applies = applies.Where(x => x.Job.Id == JobId).ToList();
                    JobIdTitle = "\"" + db.Jobs.Find(JobId).JobTitle + "\"" + " Applies";
                }

                if (CategoryId != null)
                {
                    applies = applies.Where(x => x.Job.CategoryId == CategoryId).ToList();
                    category = db.Categories.Find(CategoryId).CategoryName;
                }

                if (JobStatus != null)
                {
                    applies = applies.Where(x => x.Job.IsSuspended == Convert.ToBoolean(JobStatus)).ToList();
                    jobstatus = JobStatus == 1 ? " Suspended" : " Active";
                }

                if (!string.IsNullOrEmpty(ApplyStatus))
                {
                    applies = applies.Where(x => x.Status == ApplyStatus).ToList();
                    apply = ApplyStatus;
                }

                var ApplyTitle = apply + " Applies of " + category + jobstatus + " Jobs";
                return Ok(new { applies, categories, JobIdTitle, ApplyTitle });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }        


        [HttpPost]
        [Route("api/Admins/CreateAdmin")]
        public IHttpActionResult CreateAdmin(RegisterViewModel admin)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    UserManager.PasswordValidator = new PasswordValidator
                    {
                        RequiredLength = 6,
                        RequireNonLetterOrDigit = true,
                        RequireDigit = true,
                        RequireLowercase = true,
                        RequireUppercase = true,
                    };

                    var user = new ApplicationUser()
                    {
                        UserName = admin.UserName,
                        Email = admin.Email,
                        UserType = admin.UserType
                    };

                    var check = UserManager.Create(user, admin.Password);
                    if (check.Succeeded)
                    {
                        UserManager.AddToRole(user.Id, admin.UserType);

                        return Ok();
                    }

                    // Add the errors of the username and password validators if creating admin is failed to the Modelstate.
                    foreach (var error in check.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                return BadRequest(ModelState);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        [Route("api/Admins/DeleteAdmin/{id}")]
        public IHttpActionResult DeleteAdmin(string id)
        {
            try
            {                
                AspNetUser admin = db.AspNetUsers.Find(id);

                if (admin == null)
                {
                    return NotFound();
                }

                return Ok(admin);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete, ActionName("DeleteAdmin")]
        [Route("api/Admins/DeleteAdmin/{id}")]
        public IHttpActionResult DeleteAdminConfirmed(string id)
        {
            try
            {
                AspNetUser admin = db.AspNetUsers.Find(id);

                if(admin == null)
                {
                    return NotFound();
                }

                db.AspNetUsers.Remove(admin);
                db.SaveChanges();
                SendEmail("DeleteAdmin", admin.Email, User.Identity.Name);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Admins/DeleteUser/{id}")]
        public IHttpActionResult DeleteUser(string id)
        {
            try
            {
                AspNetUser user = db.AspNetUsers.FirstOrDefault(x => (x.UserType == "Applicant" || x.UserType == "Publisher") && x.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }                        
        }


        [HttpDelete, ActionName("DeleteUser")]
        [Route("api/Admins/DeleteUser/{id}")]
        public IHttpActionResult DeleteUserConfirmed(string id)
        {
            try
            {
                AspNetUser user = db.AspNetUsers.FirstOrDefault(x => (x.UserType == "Applicant" || x.UserType == "Publisher") && x.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                db.AspNetUsers.Remove(user);
                db.SaveChanges();

                if (user.UserType == "Publisher")
                {
                    SendEmail("DeletePublisher", user.Email, User.Identity.Name);
                    return Ok("GetPublishers");
                }
                else if (user.UserType == "Applicant")
                {
                    SendEmail("DeleteApplicant", user.Email, User.Identity.Name);
                    return Ok("GetApplicants");
                }
                else
                {
                    return Ok("Index");
                } 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }


        [HttpGet]
        [Route("api/Admins/DetailsJobByAdmin/{id}")]
        public IHttpActionResult DetailsJobByAdmin(int id)
        {
            try
            {
                Job job = db.Jobs.Where(x => x.Id == id).FirstOrDefault();
                if (job == null)
                {
                    return NotFound();
                }

                return Ok(job);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Admins/ApplyDetails/{id}")]
        public IHttpActionResult ApplyDetails(int? id)
        {
            try
            {
                ApplyForJob AppliedJob = db.ApplyForJobs.Find(id);

                if (AppliedJob == null)
                {
                    return NotFound();
                }

                return Ok(AppliedJob);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpDelete]
        [Route("api/Admins/DeleteJobByAdmin/{id}")]
        public IHttpActionResult DeleteJobByAdmin(int id)
        {
            try
            {
                Job job = db.Jobs.Find(id);

                if (job == null)
                {
                    return NotFound();
                }

                string JobPublisherEmail = job.Publisher.Email;
                var JobImage = job.JobImage;

                db.Jobs.Remove(job);
                db.SaveChanges();

                //string fullPath = HttpContext.Current.Request.MapPath("/JobFinderWebSite/Uploads/JobImages/" + JobImage);
                string fullPath = Path.Combine(JobImagesPath, JobImage);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                SendEmail("DeleteJob", JobPublisherEmail, User.Identity.Name);

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete]
        [Route("api/Admins/DeleteApply/{id}")]
        public IHttpActionResult DeleteApply(int id)
        {
            try
            {
                var apply = db.ApplyForJobs.Find(id);

                if (apply == null)
                {
                    return NotFound();
                }

                var CV = apply.CV;

                string ApplicantEmail = apply.Applicant.Email;
                string ApplyJobTitle = apply.Job.JobTitle;

                db.ApplyForJobs.Remove(apply);
                db.SaveChanges();

                //string fullPath = HttpContext.Current.Request.MapPath("/JobFinderWebSite/Uploads/CV/" + CV);
                string fullPath = Path.Combine(CVPath, CV);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                SendEmail("DeleteApply", ApplicantEmail, User.Identity.Name, ApplyJobTitle);

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("api/Admins/SendNotification")]
        public IHttpActionResult SendNotification([FromBody]Notification not, [FromUri(BinderType = typeof(TypeConverterModelBinder))] string TargetUsers, [FromUri(BinderType = typeof(TypeConverterModelBinder))] string ReceiverEmail)
        {
            try
            {
                // See "Difference between Modelstate in MVC & WebAPI" Note to know why we use these two lines.
                // 1- First Solution.

                //ModelState.Remove("ReceiverEmail.String");
                //ModelState.Remove("TargetUsers.String");

                // 2- Second Solution
                //var TargetUsers = HttpContext.Current.Request.QueryString["TargetUsers"].ToString();
                //var ReceiverEmail = HttpContext.Current.Request.QueryString["ReceiverEmail"].ToString();



                if (string.IsNullOrEmpty(ReceiverEmail) && string.IsNullOrEmpty(TargetUsers))
                {
                    ModelState["TargetUsers"].Errors.Add("TargetUsers is required.");
                    return BadRequest(ModelState);
                }

                else if (!string.IsNullOrEmpty(ReceiverEmail))
                {
                    if (ModelState.IsValid)
                    {
                        not.SenderId = User.Identity.GetUserId();
                        not.ReceiverEmail = ReceiverEmail;
                        not.IsRead = false;
                        not.CreatedDate = DateTime.Now;
                        db.Notifications.Add(not);
                        db.SaveChanges();

                        return Ok();
                    }
                    return BadRequest(ModelState);
                }

                else
                {
                    if (ModelState.IsValid)
                    {
                        List<string> usersEmail;

                        if (TargetUsers == "PublisherAndApplicant")
                        {
                            usersEmail = db.AspNetUsers.Where(x => x.UserType == "Publisher" || x.UserType == "Applicant").Select(x => x.Email).ToList();
                        }
                        else if (TargetUsers == "Publisher" || TargetUsers == "Applicant")
                        {
                            usersEmail = db.AspNetUsers.Where(x => x.UserType == TargetUsers).Select(x => x.Email).ToList();
                        }
                        else
                        {
                            return BadRequest();
                        }

                        not.SenderId = User.Identity.GetUserId();
                        not.IsRead = false;
                        not.CreatedDate = DateTime.Now;

                        foreach (var Email in usersEmail)
                        {
                            not.ReceiverEmail = Email;
                            db.Notifications.Add(not);
                            db.SaveChanges();
                        }
                        
                        return Ok();
                    }
                    return BadRequest(ModelState);
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("api/Admins/Contact")]
        public IHttpActionResult Contact(ContactViewModel contact)
        {
            try
            {

                // See "Difference between Modelstate in MVC & WebAPI" Note to know why we use these two lines.
                // 1- First Solution.

                //ModelState.Remove("ReceiverEmail.String");
                //ModelState.Remove("TargetUsers.String");

                // 2- Second Solution
                var TargetUsers = HttpContext.Current.Request.QueryString["TargetUsers"].ToString();
                var TargetEmail = HttpContext.Current.Request.QueryString["TargetEmail"].ToString();

                if (string.IsNullOrEmpty(TargetEmail) && string.IsNullOrEmpty(TargetUsers))
                {
                    ModelState["TargetUsers"].Errors.Add("TargetUsers is required.");
                    return BadRequest(ModelState);
                }

                else if (!string.IsNullOrEmpty(TargetEmail))
                {
                    if (ModelState.IsValid)
                    {
                        var CurrentUser = db.AspNetUsers.Find(User.Identity.GetUserId());
                        SendContactEmail(CurrentUser.Email, TargetEmail, contact.Subject, contact.Message);

                        return Ok();
                    }

                    // if ModelState.IsValid == false
                    return BadRequest(ModelState);
                }

                else
                {
                    if (ModelState.IsValid)
                    {
                        var CurrentUser = db.AspNetUsers.Find(User.Identity.GetUserId());

                        if (TargetUsers == "PublishersAndApplicants")
                        {
                            var UsersEmails = db.AspNetUsers.Where(x => x.UserType == "Publisher" || x.UserType == "Applicant").Select(x => x.Email).ToList();
                            foreach (var Email in UsersEmails)
                            {
                                SendContactEmail(CurrentUser.Email, Email, contact.Subject, contact.Message);
                            }
                        }
                        else if (TargetUsers == "Publishers")
                        {
                            var UsersEmails = db.AspNetUsers.Where(x => x.UserType == "Publisher").Select(x => x.Email).ToList();
                            foreach (var Email in UsersEmails)
                            {
                                SendContactEmail(CurrentUser.Email, Email, contact.Subject, contact.Message);
                            }
                        }
                        else if (TargetUsers == "Applicants")
                        {
                            var UsersEmails = db.AspNetUsers.Where(x => x.UserType == "Applicant").Select(x => x.Email).ToList();
                            foreach (var Email in UsersEmails)
                            {
                                SendContactEmail(CurrentUser.Email, Email, contact.Subject, contact.Message);
                            }
                        }
                        else
                        {
                            return BadRequest();
                        }
                        
                        return Ok();
                    }

                    // if ModelState.IsValid == false
                    return BadRequest(ModelState);
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private void SendEmail(string status, string EmailTo, string Admin, string job = null)
        {
            MailMessage mail = new MailMessage("applicationjobfinder@gmail.com", EmailTo);
            string body;

            switch (status)
            {
                case "DeleteAdmin":
                    body = "<p>You have been dismissed from your job as Admin by SuperAdmin " + Admin + ".</p>";
                    break;

                case "DeletePublisher":
                    body = "<p>Your account have been locked due to rule violations by SuperAdmin " + Admin + ".</p>";
                    break;

                case "DeleteApplicant":
                    body = "<p>Your account have been locked due to rule violations by SuperAdmin " + Admin + ".</p>";
                    break;

                case "DeleteJob":
                    body = "<p>Your Job have been removed due to rule violations by SuperAdmin " + Admin + ".</p>";
                    break;

                case "DeleteApply":
                    body = "<p>Your Apply for job(" + job + ") have been removed due to rule violations by SuperAdmin " + Admin + ".</p>";
                    break;

                default:
                    body = "";
                    break;
            }

            mail.Body = body;
            mail.Subject = "JobFinder@Admin";
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl = true;

            // these credentials is used to send the email to the receiepent (Mail.To)
            smtp.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "applicationjobfinder@gmail.com",
                Password = "123456@Sd"
            };

            smtp.Send(mail);
        }


        private void SendContactEmail(string EmailFrom, string EmailTo, string Subject, string Body)
        {
            MailMessage mail = new MailMessage(EmailFrom, EmailTo);
            mail.Body = Body;
            mail.Subject = Subject;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl = true;
            // these credentials is used to send the email to the receiepent (Mail.To)
            smtp.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "applicationjobfinder@gmail.com",
                Password = "123456@Sd"
            };
            smtp.Send(mail);
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
