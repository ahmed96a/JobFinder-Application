using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using DataAccessLayer;
using Newtonsoft.Json;
using DataAccessLayer.Models;
using Microsoft.AspNet.Identity;

namespace JobFinderWebSite.Controllers
{
    public class HomeController : Controller
    {
        private HttpClient httpClient = new HttpClient();
        
        public HomeController()
        {
            //httpClient.BaseAddress = new Uri("http://localhost:53784/api/");
            httpClient.BaseAddress = new Uri("http://ahmed3196-001-site1.ctempurl.com/api/");
        }


        public ActionResult Index()
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            
            var responseTask = httpClient.GetAsync("APIHome/Index");
            responseTask.Wait();
            
            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    categories = Enumerable.Empty<Category>(),
                    CurrentUserPostedJobsId = Enumerable.Empty<int>(),
                    CurrentUserAppliedJobsId = Enumerable.Empty<int>()
                };

                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                ViewBag.CurrentUserPostedJobsId = anonymousObject.CurrentUserPostedJobsId;
                ViewBag.CurrentUserAppliedJobsId = anonymousObject.CurrentUserAppliedJobsId;
                return View(anonymousObject.categories);
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(Enumerable.Empty<Category>());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }

        
        public ActionResult Search(string SearchJob)
        {
            if (SearchJob == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("APIHome/Search?SearchJob=" + SearchJob);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    jobs = Enumerable.Empty<Job>(),
                    CurrentUserPostedJobsId = Enumerable.Empty<int>(),
                    CurrentUserAppliedJobsId = Enumerable.Empty<int>()
                };

                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                ViewBag.CurrentUserPostedJobsId = anonymousObject.CurrentUserPostedJobsId;
                ViewBag.CurrentUserAppliedJobsId = anonymousObject.CurrentUserAppliedJobsId;
                ViewBag.SearchJob = SearchJob; 
                return View(anonymousObject.jobs);
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(Enumerable.Empty<Job>());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        public ActionResult About()
        {
            ViewBag.Message = "JobFinder .net is created and managed by AsdSoft, a technology firm founded in 2009 and one of very few companies in the MENA region specialized in developing Innovative Online Recruitment Solutions for top enterprises and organizations. Since May 2012, we successfully served 10,000+ top companies and employers in Egypt, 1.5 MILLION CVs were viewed on our platform and 100,000+ job seekers directly hired through us. In total, 250,000+ open job vacancies were advertised and now, 500,000+ users visit our website each month looking for jobs at top Employers.";

            return View();
        }


        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactViewModel contact)
        {
            if (ModelState.IsValid)
            {
                if (Request.Cookies["BearerToken"] != null)
                {
                    var token = Request.Cookies["BearerToken"].Value;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var responseTask = httpClient.PostAsJsonAsync("APIHome/Contact", contact);
                responseTask.Wait();

                var response = responseTask.Result;
                if (response.IsSuccessStatusCode)
                {
                    var status = response.Content.ReadAsStringAsync().Result;
                    ViewBag.status = status;
                    return View();
                }
                else
                {
                    string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                    ModelState.AddModelError("", ErrorMessage);
                    return View(new ContactViewModel());
                    //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
                }
            }

            return View(contact);
        }


        public ActionResult PublisherPostedJobs(string publisherName)
        {
            if (publisherName == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("APIHome/PublisherPostedJobs?publisherName=" + publisherName);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    jobs = Enumerable.Empty<Job>(),
                    CurrentUserAppliedJobsId = Enumerable.Empty<int>()
                };

                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                var jobs = anonymousObject.jobs;
                ViewBag.CurrentUserAppliedJobsId = anonymousObject.CurrentUserAppliedJobsId;
                ViewBag.Publisher = publisherName;

                return View(jobs);
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(Enumerable.Empty<Job>());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }


        [ChildActionOnly]
        public PartialViewResult NotificationData()
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("APIHome/NotificationData");
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    TotalNots = 0,
                    NewNots = 0,
                    LastNewNotTime = string.Empty
                };

                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                ViewBag.TotalNots = anonymousObject.TotalNots;
                ViewBag.NewNots = anonymousObject.NewNots;
                ViewBag.LastNewNotTime = anonymousObject.LastNewNotTime;

                return PartialView("_NotificationPartial");
            }
            else
            {
                ViewBag.TotalNots = 0;
                ViewBag.NewNots = 0;
                ViewBag.LastNewNotTime = "0 sec";

                return PartialView("_NotificationPartial");
            }
        }


        [Authorize(Roles = "Publisher, Applicant")]
        public ActionResult GetNotifications(string status)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("APIHome/GetNotifications?status=" + status);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var anonymousType = new
                {
                    nots = Enumerable.Empty<Notification>(),
                    NewNot = false
                };

                var anonymousObject = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, anonymousType);

                var nots = anonymousObject.nots;
                ViewBag.NewNot = anonymousObject.NewNot;

                return View(nots);
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(Enumerable.Empty<Notification>());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }


        }


        [Authorize(Roles = "Publisher, Applicant")]
        public ActionResult OpenNotification(int? id)
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

            var responseTask = httpClient.GetAsync("APIHome/OpenNotification/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<Notification>();
                readTask.Wait();

                var not = readTask.Result;                
                return View(not);
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(new Notification());
                //return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }


        }


        [Authorize(Roles = "Publisher, Applicant")]
        public ActionResult MarkNotAsRead(int? id)
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

            var responseTask = httpClient.GetAsync("APIHome/MarkNotAsRead/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetNotifications");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                string ErrorMessage = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError("", ErrorMessage);
                return View(Enumerable.Empty<Notification>());
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