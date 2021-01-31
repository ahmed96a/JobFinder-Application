using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using DataAccessLayer;
using Microsoft.AspNet.Identity;
using System.Web;
using System.IO;
using System.Collections.Specialized;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "Publisher")]
    public class JobsController : ApiController
    {
        //--------------------------------------------------------

        // 1- db.Configuration.ProxyCreationEnabled = false;
        //    Disable creating of EF proxy entities (that support lazy loading) that wraps the POCO Entity (in that case Category Object) in that action method {see EF dynamic proxies note}.
        //    There is no need to disable dynamic proxy entities, since we use that line of code in WebApi.config {GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;}

        // 2- ResponseTypeAttribute: - https://stackoverflow.com/questions/45875265/what-is-the-main-use-of-responsetypeattribute-in-asp-net-web-api  ,,  https://docs.microsoft.com/en-us/previous-versions/aspnet/dn308850(v%3Dvs.118)

        //--------------------------------------------------------

        private JobFinderDbContext db = new JobFinderDbContext();
        //private string JobImagesPath;

        public JobsController()
        {
            #region For Localhost
            // we can replace these lines by providing directly the url path of the JobImages folder, like "http://localhost:51732/Uploads/JobImages/"
            // also we used to save the images in the front project and we do that from the api project, and that is wrong because the uploads and the resuorces should be in the api server, so if we use different clients rather than the web, like mobile for example, these resources should be available to the client mobile.
            // also from technically point, we can save resource in the front project from the api server project if they have the same root (share the same server) that is achieved in localhost, but that not achieved when we published the two projects, because we published them on different servers, so when we try to save file from api project in the front project, we get exception (The SaveAs method is configured to require a rooted path, and the path 'fp' is not rooted).

            //var WebAPIPath = HttpContext.Current.Server.MapPath("~");
            //var SolutionPathIndex = WebAPIPath.LastIndexOf("WebAPI");
            //var SolutionPath = WebAPIPath.Substring(0, SolutionPathIndex);

            //JobImagesPath = Path.Combine(SolutionPath, @"JobFinderWebSite\Uploads\JobImages\");
            #endregion
        }

        // GET: api/Jobs
        [AllowAnonymous]        
        [ResponseType(typeof(IEnumerable<Job>))]        
        public IHttpActionResult GetJobs(int? CategoryId = null)
        {
            try
            {
                //--
                string CategoryTitle = "All";
                var Categories = db.Categories.ToList();  // {or use} var CategoriesId = new System.Web.Mvc.SelectList(db.Categories.ToList(), "Id", "CategoryName");                
                var JobsIQuerable = db.Jobs.Where(x => x.IsSuspended == false);
                var CurrentUserPostedJobsId = new List<int>();
                var CurrentUserAppliedJobsId = new List<int>();
                //--


                //-- if we filter by Category.
                if (CategoryId != null)
                {
                    JobsIQuerable = JobsIQuerable.Where(x => x.CategoryId == CategoryId);
                    CategoryTitle = db.Categories.Find(CategoryId).CategoryName;
                }

                var Jobs = JobsIQuerable.ToList();
                //--


                //-- if user is authenticated
                if (User.Identity.IsAuthenticated && (User.IsInRole("Publisher") || User.IsInRole("Applicant")))
                {
                    var CurrentUserId = User.Identity.GetUserId();
                    var CurrentUser = db.AspNetUsers.Find(CurrentUserId);
                    
                    // Get Current User Posted Jobs
                    if (CurrentUser.UserType == "Publisher")
                    {
                        CurrentUserPostedJobsId = CurrentUser.PostedJobs.Select(x => x.Id).ToList();
                    }

                    // Get Current User Applied Jobs
                    else if (CurrentUser.UserType == "Applicant")
                    {
                        CurrentUserAppliedJobsId = CurrentUser.AppliesForJobs.Select(x => x.JobId).ToList();
                    }
                }
                //--

                // return anonymous type with multiple types. (instead of creating view model that contain these types.)
                return Ok(new { Jobs, Categories, CategoryTitle, CurrentUserPostedJobsId, CurrentUserAppliedJobsId });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/Jobs/5
        [AllowAnonymous]
        [ResponseType(typeof(Job))]
        public IHttpActionResult GetJob(int id)
        {
            try
            {
                Job job = db.Jobs.Where(x => x.IsSuspended == false && x.Id == id).FirstOrDefault();

                if (job == null)
                {
                    return NotFound();                    // return Content(HttpStatusCode.NotFound, "There is No Job with this id : " + id + ".");
                }

                // Check if the user is already applied in that job or posted that job.
                if(User.Identity.IsAuthenticated)
                {
                    var  userId = User.Identity.GetUserId();
                    bool PostedThatJob = false;
                    bool AppliedToJob  = false;

                    if (User.IsInRole("Publisher"))
                    {
                        PostedThatJob = (job.PublisherId == userId);
                    }
                    else if(User.IsInRole("Applicant"))
                    {
                        AppliedToJob = job.Applies.Any(x => x.ApplicantId == userId);
                    }

                    return Ok(new { job, PostedThatJob, AppliedToJob });
                }

                return Ok(new { job });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }



        [ResponseType(typeof(Job))]
        [HttpGet]
        [Route("api/Jobs/PutJob/{id}")]
        public IHttpActionResult PutJob(int id)
        {
            try
            {
                Job job = db.Jobs.Where(x => x.Id == id).FirstOrDefault();

                if (job == null)
                {
                    return NotFound();                    // return Content(HttpStatusCode.NotFound, "There is No Job with this id : " + id + ".");
                }

                // Check if the user is already applied in that job or posted that job.
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId();
                    bool PostedThatJob = false;
                    bool AppliedToJob = false;

                    if (User.IsInRole("Publisher"))
                    {
                        PostedThatJob = (job.PublisherId == userId);
                    }
                    else if (User.IsInRole("Applicant"))
                    {
                        AppliedToJob = job.Applies.Any(x => x.ApplicantId == userId);
                    }

                    return Ok(new { job, PostedThatJob, AppliedToJob });
                }

                return Ok(new { job });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // PUT: api/Jobs/5
        [ResponseType(typeof(void))]
        [HttpPut]
        public IHttpActionResult PutJob()
        {
            try
            {                
                //1-- Check if the Request Content is in Mime Multipart Format.
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.UnsupportedMediaType, "");     //throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);        
                }
                //--

                var HttpRequest = HttpContext.Current.Request;
                Job job = new Job();


                //2-- {Using FormUrlEncodedContent in MVC}
                string jobFields = HttpRequest.Form["job"]; // or string jobFields = HttpRequest.Params["job"];
                NameValueCollection jobFieldsPairs = HttpUtility.ParseQueryString(jobFields);

                job.Id = Convert.ToInt32(jobFieldsPairs.Get("Id"));
                job.JobTitle = jobFieldsPairs.Get("JobTitle");
                job.JobContent = jobFieldsPairs.Get("JobContent");
                job.CategoryId = Convert.ToInt32(jobFieldsPairs.Get("CategoryId"));
                job.CreatedDate = DateTime.Now;
                job.PublisherId = User.Identity.GetUserId();
                //--

                //3-- Retrieve the Job object that we want to update from the database.
                var DbJob = db.Jobs.FirstOrDefault(x => x.Id == job.Id && x.PublisherId == job.PublisherId);

                if (DbJob == null)
                {
                    return NotFound();
                }

                //--

                //4-- { if the user post a new image, Then Save the Image}
                if (HttpRequest.Files.Count > 0 && !string.IsNullOrEmpty(HttpRequest.Files[0].FileName))
                {
                    HttpPostedFile JobImageFile = HttpRequest.Files[0];
                    var path = HttpRequest.MapPath("~/Uploads/JobImages/");
                    //var path = JobImagesPath;

                    var NewName = User.Identity.Name + "-" + Guid.NewGuid() + "-" + JobImageFile.FileName;
                    JobImageFile.SaveAs(Path.Combine(path, NewName));
                    job.JobImage = NewName;

                    // Delete old image
                    //string fullPath = HttpRequest.MapPath("~/JobFinderWebSite/Uploads/JobImages/" + DbJob.JobImage);
                    //string fullPath = Path.Combine(JobImagesPath, DbJob.JobImage);
                    string fullPath = Path.Combine(path, DbJob.JobImage);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                else
                {
                    // assign the name of the old image.
                    job.JobImage = DbJob.JobImage;
                }
                //--

                                

                //5-- Validate the job object again after validating it in the MVC.
                Validate(job);
                ModelState.Remove("JobImageFile");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //--

                
                //-- update the DbObject
                DbJob.JobTitle = job.JobTitle;
                DbJob.JobContent = job.JobContent;
                DbJob.CategoryId = job.CategoryId;
                DbJob.CreatedDate = DateTime.Now;
                DbJob.JobImage = job.JobImage;

                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                //--

                return Ok(job);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // POST: api/Jobs
        [ResponseType(typeof(Job))]
        public IHttpActionResult PostJob()
        {
            try
            {
                var HttpRequest = HttpContext.Current.Request;

                // Check if the Request Content is in Mime Multipart Format.
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.UnsupportedMediaType, "");     //throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);        
                }

                Job job = new Job();

                //-- {Save the File}
                if (HttpRequest.Files.Count > 0 && !string.IsNullOrEmpty(HttpRequest.Files[0].FileName))
                {
                    HttpPostedFile JobImageFile = HttpRequest.Files[0];
                    var path = HttpRequest.MapPath("~/Uploads/JobImages/");
                    //var path = JobImagesPath;

                    var NewName = User.Identity.Name + "-" + Guid.NewGuid() + "-" + JobImageFile.FileName;
                    JobImageFile.SaveAs(Path.Combine(path, NewName));
                    job.JobImage = NewName;
                }
                //--

                //-- {Using FormUrlEncodedContent in MVC}
                string jobFields = HttpRequest.Form.Get("job"); // or string jobFields = HttpRequest.Params["job"];
                NameValueCollection jobFieldsPairs = HttpUtility.ParseQueryString(jobFields);

                job.JobTitle = jobFieldsPairs.Get("JobTitle");
                job.JobContent = jobFieldsPairs.Get("JobContent");
                job.CategoryId = Convert.ToInt32(jobFieldsPairs.Get("CategoryId"));
                job.CreatedDate = DateTime.Now;
                job.PublisherId = User.Identity.GetUserId();
                //--

                //-- {Using StringContent in MVC}
                /*
                job.JobTitle = HttpRequest.Form["JobTitle"];
                job.JobContent = HttpRequest.Form["JobTitle"];
                job.CategoryId = Convert.ToInt32(HttpRequest.Form["CategoryId"]);
                job.CreatedDate = DateTime.Now;
                job.PublisherId = User.Identity.GetUserId();
                */
                //--

                Validate(job);
                ModelState.Remove("JobImageFile");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                db.Configuration.ValidateOnSaveEnabled = false;
                db.Jobs.Add(job);                       
                db.SaveChanges();

                return CreatedAtRoute("DefaultApi", new { id = job.Id }, job); // // we didn't pass an id in the job data, but when we add the job object to the database {db.SaveChanges()}, it get the id value that is autogenerated by the sql server.
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // DELETE: api/Jobs/5
        [ResponseType(typeof(Job))]
        public IHttpActionResult DeleteJob(int id)
        {
            try
            {
                var CurrentUserId = User.Identity.GetUserId();
                Job job = db.Jobs.Where(x => x.Id == id && x.PublisherId == CurrentUserId).FirstOrDefault();
                if (job == null)
                {
                    return NotFound();          //return Content(HttpStatusCode.NotFound, "Job with Id " + id.ToString() + " not found to delete");
                }

                var JobImage = job.JobImage;

                db.Jobs.Remove(job);
                db.SaveChanges();

                //string fullPath = HttpContext.Current.Request.MapPath("/JobFinderWebSite/Uploads/JobImages/" + JobImage);
                //string fullPath = Path.Combine(JobImagesPath, JobImage);
                var HttpRequest = HttpContext.Current.Request;
                var path = HttpRequest.MapPath("~/Uploads/JobImages/");
                string fullPath = Path.Combine(path, JobImage);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
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