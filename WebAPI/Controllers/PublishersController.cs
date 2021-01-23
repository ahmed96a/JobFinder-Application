using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using DataAccessLayer;
using System.IO;
using System.Web;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "Publisher")]
    public class PublishersController : ApiController
    {
        private JobFinderDbContext db = new JobFinderDbContext();

        // GET: api/Publishers/GetJobsByPublisher
        [HttpGet]
        [Route("api/Publishers/GetJobsByPublisher")]
        public IHttpActionResult GetJobsByPublisher(string JobStatus)
        {
            try
            {
                var CurrentUserId = User.Identity.GetUserId();
                List<Job> Jobs = new List<Job>();

                if (string.IsNullOrEmpty(JobStatus))
                {
                    Jobs = db.Jobs.Where(x => x.PublisherId == CurrentUserId).ToList();
                }
                else
                {
                    if (JobStatus == "Suspended Jobs")
                    {
                        Jobs = db.Jobs.Where(x => x.PublisherId == CurrentUserId && x.IsSuspended == true).ToList();
                    }

                    else if (JobStatus == "Active Jobs")
                    {
                        Jobs = db.Jobs.Where(x => x.PublisherId == CurrentUserId && x.IsSuspended == false).ToList();
                    }
                    else
                    {
                        Jobs = null;
                    }
                }

                return Ok(Jobs);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }                        
        }


        // GET: api/Publishers/GetJobDetails/5
        [HttpGet]
        [Route("api/Publishers/GetJobDetails/{id}")]
        public IHttpActionResult GetJobDetails(int? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest();
                }

                // Get the job if belong to that publisher
                var CurrentUserId = User.Identity.GetUserId();
                Job job = db.Jobs.Where(x => x.PublisherId == CurrentUserId && x.Id == id).FirstOrDefault();

                if (job == null)
                {
                    return NotFound();
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/Publishers/GetJobApplicants/5
        [HttpGet]
        [Route("api/Publishers/GetJobApplicants/{id}")]
        public IHttpActionResult GetJobApplicants(int id, string status)
        {
            try
            {
                var CurrentUserId = User.Identity.GetUserId();
                if (!db.AspNetUsers.Find(CurrentUserId).PostedJobs.Any(x => x.Id == id))
                {
                    return NotFound();
                }

                status                   = status == null ? "Pending" : status;
                var applicants           = status == "All"? db.ApplyForJobs.Where(x => x.JobId == id).ToList() : db.ApplyForJobs.Where(x => x.JobId == id && x.Status == status).ToList();
                var JobTitle             = db.Jobs.Find(id).JobTitle;
                var AllAppliesCount      = db.ApplyForJobs.Where(x => x.JobId == id).Count();
                var PendingAppliesCount  = db.ApplyForJobs.Where(x => x.JobId == id && x.Status == "Pending").Count();
                var ApprovedAppliesCount = db.ApplyForJobs.Where(x => x.JobId == id && x.Status == "Approved").Count();
                var DeniedAppliesCount   = db.ApplyForJobs.Where(x => x.JobId == id && x.Status == "Denied").Count();

                return Ok(new { applicants, JobTitle, AllAppliesCount, PendingAppliesCount, ApprovedAppliesCount, DeniedAppliesCount });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/Publishers/JobApplicantDetails/5
        [HttpGet]
        [Route("api/Publishers/JobApplicantDetails/{id}")]
        public IHttpActionResult JobApplicantDetails(int id)
        {
            try
            {
                var Apply = db.ApplyForJobs.Find(id);

                // check if the job of that Apply is belong to the current (authenticated) publisher
                var CurrentUserId = User.Identity.GetUserId();
                if (Apply == null || Apply.Job.PublisherId != CurrentUserId)
                {
                    return NotFound();
                }                

                return Ok(Apply);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }        


        [HttpGet]
        [Route("api/Publishers/SuspendJob/{id}")]
        public IHttpActionResult SuspendJob(int id)
        {
            try
            {
                // Get the job if belong to that publisher
                var CurrentUserId = User.Identity.GetUserId();
                Job job = db.Jobs.Where(x => x.PublisherId == CurrentUserId && x.Id == id).FirstOrDefault();

                if (job == null)
                {
                    return NotFound();
                }

                job.IsSuspended = true;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Publishers/ActivateJob/{id}")]
        public IHttpActionResult ActivateJob(int id)
        {
            try
            {
                // Get the job if belong to that publisher
                var CurrentUserId = User.Identity.GetUserId();
                Job job = db.Jobs.Where(x => x.PublisherId == CurrentUserId && x.Id == id).FirstOrDefault();

                if (job == null)
                {
                    return NotFound();
                }

                job.IsSuspended = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("api/Publishers/ApproveApply/{id}")]
        public IHttpActionResult ApproveApply(int id)
        {
            try
            {
                ApplyForJob Apply = db.ApplyForJobs.Find(id);

                // check if the job of that Apply is belong to the current (authenticated) publisher
                var CurrentUserId = User.Identity.GetUserId();
                if (Apply == null || Apply.Job.PublisherId != CurrentUserId)
                {
                    return NotFound();
                }

                Apply.Status = "Approved";
                db.SaveChanges();

                return Ok(Apply.JobId);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }


        [HttpGet]
        [Route("api/Publishers/DenyApply/{id}")]
        public IHttpActionResult DenyApply(int id)
        {
            try
            {
                ApplyForJob Apply = db.ApplyForJobs.Find(id);

                // check if the job of that Apply is belong to the current (authenticated) publisher
                var CurrentUserId = User.Identity.GetUserId();
                if (Apply == null || Apply.Job.PublisherId != CurrentUserId)
                {
                    return NotFound();
                }

                Apply.Status = "Denied";
                db.SaveChanges();

                return Ok(Apply.JobId);
            }
            catch (Exception ex)
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
