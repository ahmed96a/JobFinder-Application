using DataAccessLayer;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace JobFinderWebSite.Controllers
{
    [Authorize(Roles = "Applicant")]
    public class ApplicantsController : Controller
    {
        HttpClient httpClient = new HttpClient();
        
        //Uri baseAddress = new Uri("http://localhost:53784/api/");
        Uri baseAddress = new Uri("http://ahmed3196-001-site1.ctempurl.com/api/");

        public ApplicantsController()
        {
            httpClient.BaseAddress = baseAddress;
        }


        // GET: Applicants
        public ActionResult JobsThatAppliedTo()
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Applicants/JobsThatAppliedTo");
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<List<ApplyForJob>>();
                readTask.Wait();

                var AppliedJobs = readTask.Result;                
                return View(AppliedJobs);
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
                return View(Enumerable.Empty<ApplyForJob>());

                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }


        public ActionResult Apply(int? id)
        {
            // Check if the JobId is exist.
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Applicants/Apply/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                ViewBag.JobId = id;
                return View();
            }
            else if(response.StatusCode == HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else if(response.StatusCode == HttpStatusCode.Found) // Aleady Applied to that job
            {
                return RedirectToAction("Details", "Jobs", new { id = id });
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
                return View();
                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply([Bind(Include = "JobId,Message,CV")]ApplyForJob ApplyJob, HttpPostedFileBase CV)
        {
            //-- Validate the uploaded file.
            if (CV != null)
            {
                string fileExtension = Path.GetExtension(CV.FileName);
                if (fileExtension.ToLower() == ".pdf" || fileExtension.ToLower() == ".doc" || fileExtension.ToLower() == ".docx")
                {
                    int fileSize = CV.ContentLength / 1024 / 1024;
                    if (fileSize > 2)
                    {
                        ModelState.AddModelError("CV", "The Uploaded CV must be less than or equal 2 MB.");
                    }
                }
                else
                {
                    ModelState.AddModelError("CV", "The Uploaded CV must be one of that format (pdf, doc, docx).");
                }
            }
            //--

            if (!ModelState.IsValid)
            {
                return View(ApplyJob);
            }

            using (var formData = new MultipartFormDataContent())
            {
                //--- Read The Image File in byte[].
                byte[] cvData;

                using (var reader = new BinaryReader(CV.InputStream))
                {
                    cvData = reader.ReadBytes(CV.ContentLength);
                }
                //--

                //-- Get the other job properties.
                Dictionary<string, string> formFields = new Dictionary<string, string>();
                formFields.Add("JobId", ApplyJob.JobId.ToString());
                formFields.Add("Message", ApplyJob.Message);                

                FormUrlEncodedContent formFieldsContent = new FormUrlEncodedContent(formFields);
                //--

                formData.Add(new ByteArrayContent(cvData), "CV", CV.FileName);
                formData.Add(formFieldsContent, "ApplyJob");
                
                if (Request.Cookies["BearerToken"] != null)
                {
                    var token = Request.Cookies["BearerToken"].Value;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var ResponseTask = httpClient.PostAsync("Applicants/Apply", formData);
                ResponseTask.Wait();

                var response = ResponseTask.Result;
                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Result = "You Applied Successfully To This Job. You will be redirect to the Home Pgae after 3 Seconds.";
                    return View(ApplyJob);
                }
                else if (response.StatusCode == HttpStatusCode.Found) // Aleady Applied to that job
                {
                    return RedirectToAction("Details", "Jobs", new { id = ApplyJob.JobId });
                }
                else
                {
                    ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);                        
                    return View(ApplyJob);
                }
            }                       
        }


        public ActionResult ApplyDetails(int? id)
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

            var responseTask = httpClient.GetAsync("Applicants/ApplyDetails/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<ApplyForJob>();
                readTask.Wait();

                var AppliedJob = readTask.Result;
                return View(AppliedJob);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
                return View(new ApplyForJob());
                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
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


        public ActionResult DeleteApply(int? id)
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

            var responseTask = httpClient.GetAsync("Applicants/ApplyDetails/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<ApplyForJob>();
                readTask.Wait();

                var AppliedJob = readTask.Result;
                return View(AppliedJob);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }            
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
                return View();
                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteApply(int id)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.DeleteAsync($"Applicants/DeleteApply/{id}");
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("JobsThatAppliedTo");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
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


        
        public ActionResult EditApply(int? id)
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

            var responseTask = httpClient.GetAsync("Applicants/ApplyDetails/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<ApplyForJob>();
                readTask.Wait();

                var AppliedJob = readTask.Result;
                return View(AppliedJob);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
                return View();
                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]        
        public ActionResult EditApply([Bind(Include = "Id,Message,CV")]ApplyForJob ApplyJob, HttpPostedFileBase CV)
        {
            ModelState.Remove("CV"); // To remove the error CV is required if we didn't upload a new cv
            //-- if we upload a new CV, then Validate the uploaded file.
            if (CV != null)
            {
                string fileExtension = Path.GetExtension(CV.FileName);
                if (fileExtension.ToLower() == ".pdf" || fileExtension.ToLower() == ".doc" || fileExtension.ToLower() == ".docx")
                {
                    int fileSize = CV.ContentLength / 1024 / 1024;
                    if (fileSize > 2)
                    {
                        ModelState.AddModelError("CV", "The Uploaded CV must be less than or equal 2 MB.");
                    }
                }
                else
                {
                    ModelState.AddModelError("CV", "The Uploaded CV must be one of that format (pdf, doc, docx).");
                }
            }
            //--

            
            if (!ModelState.IsValid)
            {                
                return View(ApplyJob);
            }

            using (var formData = new MultipartFormDataContent())
            {

                //1-- Check if the Applicant post a new CV
                if (CV != null)
                {
                    // Read The CV File in byte[].
                    byte[] cvData;

                    using (var reader = new BinaryReader(CV.InputStream))
                    {
                        cvData = reader.ReadBytes(CV.ContentLength);
                    }
                    formData.Add(new ByteArrayContent(cvData), "CV", CV.FileName);
                }
                //--

                //2-- Get the other ApplyJob properties.
                Dictionary<string, string> formFields = new Dictionary<string, string>();
                formFields.Add("Id", ApplyJob.Id.ToString());
                formFields.Add("Message", ApplyJob.Message);

                FormUrlEncodedContent formFieldsContent = new FormUrlEncodedContent(formFields);                
                formData.Add(formFieldsContent, "ApplyJob");
                //--                


                if (Request.Cookies["BearerToken"] != null)
                {
                    var token = Request.Cookies["BearerToken"].Value;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var ResponseTask = httpClient.PutAsync("Applicants/EditApply", formData);
                ResponseTask.Wait();

                var response = ResponseTask.Result;
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("JobsThatAppliedTo");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return HttpNotFound();
                }
                else
                {                    
                        ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                        return View(ApplyJob);
                }
            }
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                httpClient.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}