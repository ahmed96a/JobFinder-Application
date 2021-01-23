using DataAccessLayer;
using DataAccessLayer.Models;
using JobFinderWebSite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace JobFinderWebSite.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AdminsController : Controller
    {
        private HttpClient httpClient = new HttpClient();
        Uri baseAddress = new Uri("http://localhost:53784/api/");

        public AdminsController()
        {
            httpClient.BaseAddress = baseAddress;
        }

        // GET: Admins
        public ActionResult Index()
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/Index");
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    PublishersCount   = 0,
                    ApplicantsCount   = 0,
                    AdminsCount       = 0,
                    JobsCount         = 0,
                    ApplyForJobsCount = 0,
                    CategoriesCount   = 0
                };

                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                ViewBag.PublishersCount   = anonymousObject.PublishersCount;
                ViewBag.ApplicantsCount   = anonymousObject.ApplicantsCount;
                ViewBag.AdminsCount       = anonymousObject.AdminsCount;
                ViewBag.JobsCount         = anonymousObject.JobsCount;
                ViewBag.ApplyForJobsCount = anonymousObject.ApplyForJobsCount;
                ViewBag.CategoriesCount = anonymousObject.CategoriesCount;

                return View();
            }
            else
            {                
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult GetAdmins(string search)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/GetAdmins?search=" + search);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {                
                var readTask = response.Content.ReadAsAsync<List<AspNetUser>>();
                readTask.Wait();

                var admins = readTask.Result;
                ViewBag.Search = search;
                return View(admins);
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult GetPublishers(string search)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/GetPublishers?search=" + search);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<List<AspNetUser>>();
                readTask.Wait();

                var publishers = readTask.Result;
                ViewBag.Search = search;
                return View(publishers);
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult GetApplicants(string search)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/GetApplicants?search=" + search);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<List<AspNetUser>>();
                readTask.Wait();

                var applicants = readTask.Result;
                ViewBag.Search = search;
                return View(applicants);
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult GetCategories(string search)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/GetCategories?search=" + search);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<List<Category>>();
                readTask.Wait();

                var categories = readTask.Result;
                ViewBag.Search = search;
                return View(categories);
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult GetJobs(int? CategoryId, int? JobStatus, string Publisher, string Search)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/GetJobs?CategoryId=" + CategoryId + "&JobStatus=" + JobStatus + "&Publisher=" + Publisher + "&Search=" + Search);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    categories = Enumerable.Empty<Category>(),
                    jobs       = Enumerable.Empty<Job>(),
                    JobTitle   = string.Empty
                };
                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                ViewBag.CategoryId = new SelectList(anonymousObject.categories, "Id", "CategoryName");
                ViewBag.JobStatus = new List<SelectListItem> { new SelectListItem() { Text = "Active", Value = "0" }, new SelectListItem() { Text = "Suspend", Value = "1" } };
                ViewBag.JobTitle = anonymousObject.JobTitle;
                ViewBag.Search = Search;
                ViewBag.Publisher = Publisher;

                return View(anonymousObject.jobs);
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult GetApplies(int? CategoryId, int? JobStatus, string ApplyStatus, string Applicant, int? JobId, string search)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/GetApplies?CategoryId=" + CategoryId + "&JobStatus=" + JobStatus + "&ApplyStatus=" + ApplyStatus + "&Applicant=" + Applicant + "&JobId=" + JobId + "&search=" + search);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    applies = Enumerable.Empty<ApplyForJob>(),
                    categories = Enumerable.Empty<Category>(),
                    JobIdTitle = string.Empty,
                    ApplyTitle = string.Empty,
                };
                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);
                
                ViewBag.Search = search;
                ViewBag.Applicant = string.IsNullOrEmpty(Applicant) ? string.Empty : "\"" + Applicant + "\"" + " Applies";
                ViewBag.JobIdTitle = anonymousObject.JobIdTitle;
                ViewBag.CategoryId = new SelectList(anonymousObject.categories, "Id", "CategoryName");
                ViewBag.JobStatus = new List<SelectListItem> { new SelectListItem() { Text = "Active Jobs", Value = "0" }, new SelectListItem() { Text = "Suspended Jobs", Value = "1" } };
                ViewBag.ApplyStatus = new List<SelectListItem> { new SelectListItem() { Text = "Pending Applies", Value = "Pending" }, new SelectListItem() { Text = "Approved Applies", Value = "Approved" }, new SelectListItem() { Text = "Denied Applies", Value = "Denied" } };
                ViewBag.ApplyTitle = anonymousObject.ApplyTitle;

                return View(anonymousObject.applies);
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult DownloadCV(string CVName, string ApplicantName, string JobName)
        {
            string Extension = CVName.Substring(CVName.LastIndexOf(".") + 1).ToLower();
            switch (Extension)
            {
                case "doc":
                    return File("~/Uploads/CV/" + CVName, "application/msword", JobName + "-" + ApplicantName + "-CV." + Extension);

                case "docx":
                    return File("~/Uploads/CV/" + CVName, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", JobName + "-" + ApplicantName + "-CV." + Extension);

                case "pdf":
                default:
                    return File("~/Uploads/CV/" + CVName, "application/pdf", JobName + "-" + ApplicantName + "-CV." + Extension);
            }
        }


        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public ActionResult CreateAdmin()
        {
            ViewBag.UserType = new List<SelectListItem> { new SelectListItem { Text = "SuperAdmin", Value = "SuperAdmin" }, new SelectListItem { Text = "Admin", Value = "Admin" } };
            return View();
        }


        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin([Bind(Include = "UserName,UserType,Email,Password,ConfirmPassword")]RegisterViewModel admin)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserType = new List<SelectListItem> { new SelectListItem { Text = "SuperAdmin", Value = "SuperAdmin" }, new SelectListItem { Text = "Admin", Value = "Admin" } };
                return View(admin);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.PostAsJsonAsync<RegisterViewModel>("Admins/CreateAdmin", admin);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetAdmins");
            }
            else
            {
                ViewBag.UserType = new List<SelectListItem> { new SelectListItem { Text = "SuperAdmin", Value = "SuperAdmin" }, new SelectListItem { Text = "Admin", Value = "Admin" } };
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View(admin);
            }
        }


        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public ActionResult DeleteAdmin(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/DeleteAdmin/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<AspNetUser>();
                readTask.Wait();

                var admin = readTask.Result;
                return View(admin);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("DeleteAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAdminConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.DeleteAsync("Admins/DeleteAdmin/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetAdmins");
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        [HttpGet]
        public ActionResult DeleteUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/DeleteUser/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<AspNetUser>();
                readTask.Wait();

                var user = readTask.Result;
                ViewBag.User = user.UserType;
                return View(user);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUserConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.DeleteAsync("Admins/DeleteUser/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var redirectAction = response.Content.ReadAsStringAsync().Result.Trim('"');
                return RedirectToAction(redirectAction);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult DetailsJobByAdmin(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/DetailsJobByAdmin/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
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
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult ApplyDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/ApplyDetails/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<ApplyForJob>();
                readTask.Wait();

                var AppliedJob = readTask.Result;
                return View(AppliedJob);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }



        public ActionResult DeleteJobByAdmin(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/DetailsJobByAdmin/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
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
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        [HttpPost, ActionName("DeleteJobByAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteJobByAdminConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.DeleteAsync("Admins/DeleteJobByAdmin/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetJobs", "Admins");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }



        public ActionResult DeleteApply(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Admins/ApplyDetails/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<ApplyForJob>();
                readTask.Wait();

                var AppliedJob = readTask.Result;
                return View(AppliedJob);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }



        [HttpPost, ActionName("DeleteApply")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteApplyConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.DeleteAsync("Admins/DeleteApply/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetApplies", "Admins");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }



        [HttpGet]
        public ActionResult SendNotification(string ReceiverEmail)
        {
            if (ReceiverEmail != null)
            {
                ViewBag.ReceiverEmail = ReceiverEmail;
            }
            else
            {
                ViewBag.TargetUsers = new List<SelectListItem> { new SelectListItem { Text = "Publishers And Applicants", Value = "PublisherAndApplicant" }, new SelectListItem { Text = "Publishers", Value = "Publisher" }, new SelectListItem { Text = "Applicants", Value = "Applicant" } };
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendNotification([Bind(Include = "Subject, Message")]Notification not, string TargetUsers, string ReceiverEmail)
        {
            if (string.IsNullOrEmpty(ReceiverEmail) && string.IsNullOrEmpty(TargetUsers))
            {
                ModelState["TargetUsers"].Errors.Add("TargetUsers is required.");
            }

            if (!ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(ReceiverEmail))
                {
                    ViewBag.ReceiverEmail = ReceiverEmail;
                }
                else
                {
                    ViewBag.TargetUsers = new List<SelectListItem> { new SelectListItem { Text = "Publishers And Applicants", Value = "PublisherAndApplicant" }, new SelectListItem { Text = "Publishers", Value = "Publisher" }, new SelectListItem { Text = "Applicants", Value = "Applicant" } };
                }
                return View(not);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            
            var responseTask = httpClient.PostAsJsonAsync("Admins/SendNotification?TargetUsers=" + TargetUsers + "&ReceiverEmail=" + ReceiverEmail, not);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(ReceiverEmail))
                {
                    ViewBag.ReceiverEmail = ReceiverEmail;
                    ViewBag.success = "The Notification is successfully sent.";

                    return View();
                }
                else
                {
                    ViewBag.TargetUsers = new List<SelectListItem> { new SelectListItem { Text = "Publishers And Applicants", Value = "PublisherAndApplicant" }, new SelectListItem { Text = "Publishers", Value = "Publisher" }, new SelectListItem { Text = "Applicants", Value = "Applicant" } };
                    ViewBag.success = "The Notification is successfully sent.";

                    return View();
                }

            }            
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                if (!string.IsNullOrEmpty(ReceiverEmail))
                {
                    ViewBag.ReceiverEmail = ReceiverEmail;
                }
                else
                {
                    ViewBag.TargetUsers = new List<SelectListItem> { new SelectListItem { Text = "Publishers And Applicants", Value = "PublisherAndApplicant" }, new SelectListItem { Text = "Publishers", Value = "Publisher" }, new SelectListItem { Text = "Applicants", Value = "Applicant" } };
                    if(!string.IsNullOrEmpty(TargetUsers))
                    {
                        ViewBag.error = "An error is occured.";
                    }
                }

                return View(not);
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        [HttpGet]
        public ActionResult Contact(string TargetEmail)
        {
            if (TargetEmail != null)
            {
                ViewBag.TargetEmail = TargetEmail;
            }
            else
            {
                ViewBag.TargetUsers = new List<SelectListItem> { new SelectListItem { Text = "Publishers And Applicants", Value = "PublishersAndApplicants" }, new SelectListItem { Text = "Publishers", Value = "Publishers" }, new SelectListItem { Text = "Applicants", Value = "Applicants" } };
            }
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact([Bind(Include = "Subject, Message")]ContactViewModel contact, string TargetUsers, string TargetEmail)
        {
            if (string.IsNullOrEmpty(TargetEmail) && string.IsNullOrEmpty(TargetUsers))
            {
                ModelState["TargetUsers"].Errors.Add("TargetUsers is required.");
            }

            if (!ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(TargetEmail))
                {
                    ViewBag.ReceiverEmail = TargetEmail;
                }
                else
                {
                    ViewBag.TargetUsers = new List<SelectListItem> { new SelectListItem { Text = "Publishers And Applicants", Value = "PublisherAndApplicant" }, new SelectListItem { Text = "Publishers", Value = "Publisher" }, new SelectListItem { Text = "Applicants", Value = "Applicant" } };
                }
                return View(contact);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.PostAsJsonAsync("Admins/Contact?TargetUsers=" + TargetUsers + "&TargetEmail=" + TargetEmail, contact);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(TargetEmail))
                {
                    ViewBag.TargetEmail = TargetEmail;
                    ViewBag.success = "The Notification is successfully sent.";

                    return View();
                }
                else
                {
                    ViewBag.TargetUsers = new List<SelectListItem> { new SelectListItem { Text = "Publishers And Applicants", Value = "PublisherAndApplicant" }, new SelectListItem { Text = "Publishers", Value = "Publisher" }, new SelectListItem { Text = "Applicants", Value = "Applicant" } };
                    ViewBag.success = "The Notification is successfully sent.";

                    return View();
                }

            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                if (!string.IsNullOrEmpty(TargetEmail))
                {
                    ViewBag.ReceiverEmail = TargetEmail;
                }
                else
                {
                    ViewBag.TargetUsers = new List<SelectListItem> { new SelectListItem { Text = "Publishers And Applicants", Value = "PublisherAndApplicant" }, new SelectListItem { Text = "Publishers", Value = "Publisher" }, new SelectListItem { Text = "Applicants", Value = "Applicant" } };
                    if (!string.IsNullOrEmpty(TargetUsers))
                    {
                        ViewBag.error = "An error is occured.";
                    }
                }

                return View(contact);
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
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