using DataAccessLayer;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
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
    [Authorize(Roles = "Publisher")]
    public class JobsController : Controller
    {
        private HttpClient httpClient = new HttpClient();

        public JobsController()
        {
            httpClient.BaseAddress = new Uri("http://localhost:53784/api/");            
        }


        // GET: Jobs
        [AllowAnonymous]
        public ActionResult Index(int? CategoryId = null)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("jobs?CategoryId=" + CategoryId);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    jobs = new List<Job>(),
                    categories = new List<Category>(),
                    CategoryTitle = string.Empty,
                    CurrentUserPostedJobsId = new List<int>(),
                    CurrentUserAppliedJobsId = new List<int>()
                };
                    
                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);
                    
                ViewBag.CategoryTitle = anonymousObject.CategoryTitle;
                ViewBag.CategoryId = new SelectList(anonymousObject.categories, "Id", "CategoryName");
                ViewBag.CurrentUserPostedJobsId = anonymousObject.CurrentUserPostedJobsId;
                ViewBag.CurrentUserAppliedJobsId = anonymousObject.CurrentUserAppliedJobsId;
                return View(anonymousObject.jobs);
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(new List<Job>());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

        }

        // GET: Jobs/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if(Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("jobs/" + id.ToString());
            ResponseTask.Wait();

            var anonymousType = new {
                job = new Job(),
                PostedThatJob = false,
                AppliedToJob = false
            };

            var response = ResponseTask.Result;
            if(response.IsSuccessStatusCode)
            {
                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                ViewBag.PostedThatJob = anonymousObject.PostedThatJob;
                ViewBag.AppliedToJob = anonymousObject.AppliedToJob;

                return View(anonymousObject.job);
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(anonymousType.job);
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        // GET: Jobs/Create
        public ActionResult Create()
        {
            using (JobFinderDbContext db = new JobFinderDbContext())
            {
                ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "Id", "CategoryName");
                return View();
            }
        }


        // POST: Jobs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // we should use AllowHtml attribute (disable input validation for specific property) instead of ValidateInput attribute but the AllowHtml attribute exist in the System.Web.MVC namespace which we cant use class library project (DataAccessLayer project), so that we used ValidateInput attribute which wil disable input validation for the entire action method.
        public ActionResult Create([Bind(Include = "Id,JobTitle,JobContent,JobImageFile,CategoryId")]Job job)
        {
            if(!ModelState.IsValid)
            {
                using (JobFinderDbContext db = new JobFinderDbContext())
                {
                    ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "Id", "CategoryName");
                    return View(job);
                }
            }
            
            using (var formData = new MultipartFormDataContent())
            {
                //--- Read The Image File in byte[].
                byte[] imgData;
                    
                using (var reader = new BinaryReader(job.JobImageFile.InputStream))
                {
                    imgData = reader.ReadBytes(job.JobImageFile.ContentLength);
                }
                //--


                //-- Get the other job properties.
                Dictionary<string, string> formFields = new Dictionary<string, string>();
                formFields.Add("JobTitle", job.JobTitle);
                formFields.Add("JobContent", job.JobContent);
                formFields.Add("CategoryId", job.CategoryId.ToString());

                FormUrlEncodedContent formFieldsContent = new FormUrlEncodedContent(formFields);
                //--

                //-- Use List of KeyValuePair<string, string> instead of dictionary.
                /*
                var formFields2 = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("JobTitle", job.JobTitle),
                    new KeyValuePair<string, string>("JobContent", job.JobContent),
                    new KeyValuePair<string, string>("CategoryId", job.CategoryId.ToString()),
                };

                FormUrlEncodedContent formFieldsContent2 = new FormUrlEncodedContent(formFields2);
                */
                //--


                formData.Add(new ByteArrayContent(imgData), "JobImageFile", job.JobImageFile.FileName);
                formData.Add(formFieldsContent, "job");

                //-- Using StringContent instead of FormUrlEncodedContent
                /*
                formData.Add(new StringContent(job.JobTitle), "JobTitle");
                formData.Add(new StringContent(HttpUtility.HtmlEncode(job.JobContent)), "JobContent");
                formData.Add(new StringContent(job.CategoryId.ToString()), "CategoryId");
                */
                //--

                if (Request.Cookies["BearerToken"] != null)
                {
                    var token = Request.Cookies["BearerToken"].Value;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var ResponseTask = httpClient.PostAsync("jobs", formData);
                ResponseTask.Wait();

                var response = ResponseTask.Result;

                if(response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                using (JobFinderDbContext db = new JobFinderDbContext())
                {
                    ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                    ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "Id", "CategoryName");
                    return View(job);
                }
            }
        }



        // GET: Jobs/Edit/5
        public ActionResult Edit(int? id)
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

            var ResponseTask = httpClient.GetAsync($"jobs/PutJob/{id}");
            ResponseTask.Wait();

            var anonymousType = new
            {
                job = new Job(),
                PostedThatJob = false,
                AppliedToJob = false
            };

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                if(!anonymousObject.PostedThatJob)
                {
                    return HttpNotFound();
                }

                using (JobFinderDbContext db = new JobFinderDbContext())
                {
                    ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "Id", "CategoryName", anonymousObject.job.CategoryId);
                }
                return View(anonymousObject.job);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(anonymousType.job);
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        // POST: Jobs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "Id,JobTitle,JobContent,JobImageFile,CategoryId")]Job job)
        {
            ModelState.Remove("JobImageFile"); // remove the Validation message of JobImageFile property "The JobImageFile is required", in case the user isn't upload a new image.
            if (!ModelState.IsValid)
            {
                using (JobFinderDbContext db = new JobFinderDbContext())
                {
                    ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "Id", "CategoryName", job.CategoryId);
                }
                return View(job);
            }

            using (var formData = new MultipartFormDataContent())
            {

                //1-- Check if the User post a new image
                if (job.JobImageFile != null)
                {
                    // Read The Image File in byte[].
                    byte[] imgData;

                    using (var reader = new BinaryReader(job.JobImageFile.InputStream))
                    {
                        imgData = reader.ReadBytes(job.JobImageFile.ContentLength);
                    }
                    formData.Add(new ByteArrayContent(imgData), "JobImageFile", job.JobImageFile.FileName);
                }
                //--

                //2-- Get the other job properties.
                Dictionary<string, string> formFields = new Dictionary<string, string>();
                formFields.Add("Id", job.Id.ToString());
                formFields.Add("JobTitle", job.JobTitle);
                formFields.Add("JobContent", job.JobContent);
                formFields.Add("CategoryId", job.CategoryId.ToString());

                FormUrlEncodedContent formFieldsContent = new FormUrlEncodedContent(formFields);
                formData.Add(formFieldsContent, "job");
                //--                


                if (Request.Cookies["BearerToken"] != null)
                {
                    var token = Request.Cookies["BearerToken"].Value;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var ResponseTask = httpClient.PutAsync("jobs", formData);
                ResponseTask.Wait();

                var response = ResponseTask.Result;
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    return HttpNotFound();
                }
                else
                {
                    using (JobFinderDbContext db = new JobFinderDbContext())
                    {
                        ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                        ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "Id", "CategoryName", job.CategoryId);
                        return View(job);
                    }
                }                
            }
        }



        // GET: Jobs/Delete/5
        public ActionResult Delete(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var ResponseTask = httpClient.GetAsync("jobs/PutJob/" + id.ToString());
            ResponseTask.Wait();

            var anonymousType = new
            {
                job = new Job(),
                PostedThatJob = false,
                AppliedToJob = false
            };

            var response = ResponseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                // if that job not published by that user.
                if (anonymousObject.job.PublisherId != User.Identity.GetUserId()) // or if (!anonymousObject.PostedThatJob)
                {
                    return HttpNotFound();
                }

                return View(anonymousObject.job);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(anonymousType.job);
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }

        // POST: Jobs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.DeleteAsync($"Jobs/{id}");
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
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
