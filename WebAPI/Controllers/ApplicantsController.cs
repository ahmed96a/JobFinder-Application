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
using System.Collections.Specialized;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "Applicant")]
    public class ApplicantsController : ApiController
    {
        private JobFinderDbContext db = new JobFinderDbContext();
        private string CVPath;

        public ApplicantsController()
        {
            var WebAPIPath = HttpContext.Current.Server.MapPath("~");
            var SolutionPathIndex = WebAPIPath.LastIndexOf("WebAPI");
            var SolutionPath = WebAPIPath.Substring(0, SolutionPathIndex);

            CVPath = Path.Combine(SolutionPath, @"JobFinderWebSite\Uploads\CV\");
        }

        [HttpGet]
        [Route("api/Applicants/JobsThatAppliedTo")]
        public IHttpActionResult JobsThatAppliedTo()
        {
            try
            {
                var CurrentUserId = User.Identity.GetUserId();
                var AppliedJobs = db.ApplyForJobs.Where(x => x.ApplicantId == CurrentUserId).ToList();

                return Ok(AppliedJobs);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Applicants/Apply/{id}", Name = "Apply")]
        public IHttpActionResult Apply(int id)
        {
            try
            {
                // Check if the Job is exist and not suspended.
                var job = db.Jobs.Any(x => x.Id == id && x.IsSuspended == false);

                if (!job)
                {
                    return NotFound();
                }

                // Check if the user is already applied in that job.
                var CurrentUserId = User.Identity.GetUserId();
                bool AlreadyApplied = db.ApplyForJobs.Any(x => x.ApplicantId == CurrentUserId && x.JobId == id);

                if (AlreadyApplied)
                {                    
                    return Content(HttpStatusCode.Found, "");
                }

                return Ok();                
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("api/Applicants/Apply")]
        public IHttpActionResult Apply()
        {
            try
            {
                var HttpRequest = HttpContext.Current.Request;

                // Check if the Request Content is in Mime Multipart Format.
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.UnsupportedMediaType, "");     //throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);        
                }

                ApplyForJob ApplyJob = new ApplyForJob();

                //-- {Save the File}
                if (HttpRequest.Files.Count > 0 && !string.IsNullOrEmpty(HttpRequest.Files[0].FileName))
                {
                    HttpPostedFile CV = HttpRequest.Files[0];
                    //var path = HttpRequest.MapPath("/JobFinderWebSite/Uploads/CV/");
                    var path = CVPath;

                    var NewName = Guid.NewGuid() + "-" + User.Identity.Name + "-" + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "-" + CV.FileName;
                    CV.SaveAs(Path.Combine(path, NewName));
                    ApplyJob.CV = NewName;
                }
                //--

                //-- {Using FormUrlEncodedContent in MVC}

                string ApplyJobFields = HttpRequest.Form.Get("ApplyJob"); // or string jobFields = HttpRequest.Params["job"];
                NameValueCollection ApplyJobFieldsPairs = HttpUtility.ParseQueryString(ApplyJobFields);

                ApplyJob.JobId     = Convert.ToInt32(ApplyJobFieldsPairs.Get("JobId"));
                ApplyJob.Message   = ApplyJobFieldsPairs.Get("Message");
                ApplyJob.ApplyDate = DateTime.Now;
                ApplyJob.Status = "Pending";
                ApplyJob.ApplicantId = User.Identity.GetUserId();                
                
                //--
                
                Validate(ApplyJob);                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // we add that block to prevent the user from apply again in the same job, if the user click apply button again before redirection to Job list after first apply.
                //-----------------
                bool AlreadyApplied = db.ApplyForJobs.Any(x => x.ApplicantId == ApplyJob.ApplicantId && x.JobId == ApplyJob.JobId);

                if (AlreadyApplied)
                {
                    return Content(HttpStatusCode.Found, "");
                }
                //-----------------

                //db.Configuration.ValidateOnSaveEnabled = false;
                db.ApplyForJobs.Add(ApplyJob);
                db.SaveChanges();

                var location = new Uri(Url.Link("Apply", new { id = ApplyJob.Id }));
                return Created(location, ApplyJob); // we didn't pass an id in the ApplyJob data, but when we add the ApplyJob object to the database {db.SaveChanges()}, it get the id value that is autogenerated by the sql server.
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Applicants/ApplyDetails/{id}")]
        public IHttpActionResult ApplyDetails(int id)
        {
            try
            {
                var CurrentUserId = User.Identity.GetUserId();
                ApplyForJob AppliedJob = db.ApplyForJobs.Where(x => x.Id == id && x.ApplicantId == CurrentUserId).FirstOrDefault();

                if (AppliedJob == null)
                {
                    return NotFound();
                }

                return Ok(AppliedJob);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }        


        [HttpDelete]
        [Route("api/Applicants/DeleteApply/{id}")]
        public IHttpActionResult DeleteApply(int id)
        {
            try
            {
                var CurrentUserId = User.Identity.GetUserId();
                ApplyForJob ApplyJob = db.ApplyForJobs.Where(x => x.Id == id && x.ApplicantId == CurrentUserId).FirstOrDefault();
                if (ApplyJob == null)
                {
                    return NotFound();          //return Content(HttpStatusCode.NotFound, "Job with Id " + id.ToString() + " not found to delete");
                }

                var CV = ApplyJob.CV;

                db.ApplyForJobs.Remove(ApplyJob);
                db.SaveChanges();

                //string fullPath = HttpContext.Current.Request.MapPath("/JobFinderWebSite/Uploads/CV/" + CV);
                string fullPath = Path.Combine(CVPath, CV);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut]
        [Route("api/Applicants/EditApply")]
        public IHttpActionResult EditApply()
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

                //2-- Retrieve the ApplyJob Properties. {Using FormUrlEncodedContent in MVC}

                string ApplyJobFields = HttpRequest.Form.Get("ApplyJob"); // or string jobFields = HttpRequest.Params["job"];
                NameValueCollection ApplyJobFieldsPairs = HttpUtility.ParseQueryString(ApplyJobFields);

                var ApplyJobId = Convert.ToInt32(ApplyJobFieldsPairs.Get("Id"));
                var ApplyJobMessage = ApplyJobFieldsPairs.Get("Message");
                var ApplyJobApplicantId = User.Identity.GetUserId();

                //--

                //3-- Retrieve the ApplyJob object that we want to update from the database.
                var DbApplyJob = db.ApplyForJobs.FirstOrDefault(x => x.Id == ApplyJobId && x.ApplicantId == ApplyJobApplicantId);

                if (DbApplyJob == null)
                {
                    return NotFound();
                }

                //--

                //4-- { if the user post a new CV, Then Save the CV}
                if (HttpRequest.Files.Count > 0 && !string.IsNullOrEmpty(HttpRequest.Files[0].FileName))
                {
                    HttpPostedFile CV = HttpRequest.Files[0];
                    //var path = HttpRequest.MapPath("/JobFinderWebSite/Uploads/CV/");
                    var path = CVPath;

                    var NewName = Guid.NewGuid() + "-" + User.Identity.Name + "-" + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "-" + CV.FileName;
                    CV.SaveAs(Path.Combine(path, NewName));

                    // Delete old CV
                    //string fullPath = HttpRequest.MapPath("/JobFinderWebSite/Uploads/CV/" + DbApplyJob.CV);
                    string fullPath = Path.Combine(CVPath, DbApplyJob.CV);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    DbApplyJob.CV = NewName;
                }

                //-- update the DbObject
                DbApplyJob.Message = ApplyJobMessage;
                DbApplyJob.ApplyDate = DateTime.Now;

                db.SaveChanges();
                //--

                return Ok(DbApplyJob);
            }
            catch (Exception ex)
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
