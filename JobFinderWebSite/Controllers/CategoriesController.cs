using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using DataAccessLayer;
using System.Net;

namespace JobFinderWebSite.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class CategoriesController : Controller
    {
        private HttpClient httpClient = new HttpClient();

        public CategoriesController()
        {
            //httpClient.BaseAddress = new Uri("http://localhost:53784/api/");
            httpClient.BaseAddress = new Uri("http://ahmed3196-001-site1.ctempurl.com/api/");
        }


        // GET: Categories
        [AllowAnonymous]
        public ActionResult Index()
        {
            IEnumerable<Category> categories;

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Categories");
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<List<Category>>();
                readTask.Wait();

                categories = readTask.Result;
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
                categories = Enumerable.Empty<Category>();
            }

            return View(categories);
        }

        // GET: Categories/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var category = new Category();

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Categories/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if(response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<Category>();
                readTask.Wait();

                category = readTask.Result;
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
            }
            return View(category);
        }



        // GET: Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CategoryName,CategoryDescription")]Category cat)
        {
            if (!ModelState.IsValid)
            {
                return View(cat);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.PostAsJsonAsync("Categories", cat);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetCategories", "Admins");
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View(cat);
            }
            
        }



        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var category = new Category();

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Categories/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<Category>();
                readTask.Wait();

                category = readTask.Result;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CategoryName,CategoryDescription")]Category cat)
        {
            if(!ModelState.IsValid)
            {
                return View(cat);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.PutAsJsonAsync("Categories", cat);
            responseTask.Wait();

            var response = responseTask.Result;
            if(response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetCategories", "Admins");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View(cat);
            }

        }



        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var category = new Category();

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("Categories/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<Category>();
                readTask.Wait();

                category = readTask.Result;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.DeleteAsync("Categories/" + id.ToString());
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetCategories", "Admins");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return HttpNotFound();
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View();
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
