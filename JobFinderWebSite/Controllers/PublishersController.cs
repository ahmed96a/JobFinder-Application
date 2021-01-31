using DataAccessLayer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace JobFinderWebSite.Controllers
{
    [Authorize(Roles = "Publisher")]
    public class PublishersController : Controller
    {
        HttpClient httpClient = new HttpClient();
        //Uri baseAddress = new Uri("http://localhost:53784/api/");
        Uri baseAddress = new Uri("http://ahmed3196-001-site1.ctempurl.com/api/");

        public PublishersController()
        {
            httpClient.BaseAddress = baseAddress;
        }

        // GET: Publishers/GetJobsByPublisher
        public ActionResult GetJobsByPublisher(string JobStatus)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Publishers/GetJobsByPublisher?JobStatus=" + JobStatus);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<List<Job>>();
                readTask.Wait();

                var jobs = readTask.Result;
                ViewBag.JobTitle = JobStatus == null ? "All Jobs" : JobStatus;
                ViewBag.JobStatus = new List<SelectListItem> { new SelectListItem() { Text = "Active Jobs", Value = "Active Jobs" }, new SelectListItem() { Text = "Suspended Jobs", Value = "Suspended Jobs" } };

                return View(jobs);
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
                return View(Enumerable.Empty<Job>());

                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }


        // GET: Publishers/GetJobDetails/{id}
        public ActionResult GetJobDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("Publishers/GetJobDetails/" + id.ToString());
            ResponseTask.Wait();            

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<Job>();
                readTask.Wait();

                var job = readTask.Result;
                return View(job);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(new Job());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        // GET: Publishers/GetJobApplicants/{id}
        public ActionResult GetJobApplicants(int? id, string status)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("Publishers/GetJobApplicants/" + id.ToString() + "?status=" + status);
            ResponseTask.Wait();

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    applicants           = Enumerable.Empty<ApplyForJob>(),
                    JobTitle             = string.Empty,
                    AllAppliesCount      = 0,
                    PendingAppliesCount  = 0,
                    ApprovedAppliesCount = 0,
                    DeniedAppliesCount   = 0
                };

                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                ViewBag.JobTitle = anonymousObject.JobTitle;
                ViewBag.AllAppliesCount = anonymousObject.AllAppliesCount;
                ViewBag.PendingAppliesCount = anonymousObject.PendingAppliesCount;
                ViewBag.ApprovedAppliesCount = anonymousObject.ApprovedAppliesCount;
                ViewBag.DeniedAppliesCount = anonymousObject.DeniedAppliesCount;
                ViewBag.JobId = id;

                return View(anonymousObject.applicants);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(Enumerable.Empty<ApplyForJob>());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        // GET: Publishers/JobApplicantDetails/{id}
        public ActionResult JobApplicantDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("Publishers/JobApplicantDetails/" + id.ToString());
            ResponseTask.Wait();

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<ApplyForJob>();
                readTask.Wait();

                var Apply = readTask.Result;
                return View(Apply);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(new ApplyForJob());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult DownloadCV(string CVName, string ApplicantName, string JobName)
        {
            // We can’t download file from different server by using File() method and providing to it the absolute path of the file, because File() download file from the same server by providing virtual path to it.
            // The Resources of the that used to write the new code of download function.

            // Reference.
            // https://stackoverflow.com/questions/3604562/download-file-of-any-type-in-asp-net-mvc-using-fileresult
            // https://stackoverflow.com/questions/51615630/c-sharp-download-file-from-webservice

            string Extension = CVName.Substring(CVName.LastIndexOf(".") + 1).ToLower();

            var response = httpClient.GetAsync(@"http://ahmed3196-001-site1.ctempurl.com/Uploads/CV/" + CVName).Result;
            var result = response.Content.ReadAsByteArrayAsync().Result;

            /*
            switch (Extension)
            {
                case "doc":
                    return File(result, "application/msword", JobName + "-" + ApplicantName + "-CV." + Extension);

                case "docx":
                    return File(result, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", JobName + "-" + ApplicantName + "-CV." + Extension);

                case "pdf":
                default:
                    return File(result, "application/pdf", JobName + "-" + ApplicantName + "-CV." + Extension);
            }
            */

            return File(result, System.Net.Mime.MediaTypeNames.Application.Octet, JobName + "-" + ApplicantName + "-CV." + Extension);
        }


        public ActionResult SuspendJob(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("Publishers/SuspendJob/" + id.ToString());
            ResponseTask.Wait();

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetJobsByPublisher");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult ActivateJob(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("Publishers/ActivateJob/" + id.ToString());
            ResponseTask.Wait();

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetJobsByPublisher");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult ApproveApply(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("Publishers/ApproveApply/" + id.ToString());
            ResponseTask.Wait();

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var JobId = response.Content.ReadAsStringAsync().Result;
                return RedirectToAction("GetJobApplicants", new { id = JobId });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult DenyApply(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("Publishers/DenyApply/" + id.ToString());
            ResponseTask.Wait();

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var JobId = response.Content.ReadAsStringAsync().Result;
                return RedirectToAction("GetJobApplicants", new { id = JobId });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }
        

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                httpClient.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
