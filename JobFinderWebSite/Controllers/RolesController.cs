using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;

namespace JobFinderWebSite.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : Controller
    {
        HttpClient httpClient = new HttpClient();
        Uri baseAddress = new Uri("http://localhost:53784/api/");

        public RolesController()
        {
            httpClient.BaseAddress = baseAddress;
        }

        // GET: Roles
        public ActionResult Index()
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.GetAsync("roles");
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<List<IdentityRole>>();
                readTask.Wait();

                var roles = readTask.Result;
                return View(roles);
            }
            else
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                ModelState.AddModelError("", readTask.Result);
                return View(Enumerable.Empty<IdentityRole>());

                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }

        // GET: Roles/Details/5
        public ActionResult Details(string id)
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

            var responseTask = httpClient.GetAsync("roles/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<IdentityRole>();
                readTask.Wait();

                var roles = readTask.Result;
                return View(roles);
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
                return View(new IdentityRole());

                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id, Name")]IdentityRole role) // we use IdentityRole instead of AspNetRole table, to make asp.net identity system auto generate the id of the role.
        {
            if(role.Name == null) // we add the validation here.
            {
                ModelState.AddModelError("Name", "Name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.PostAsJsonAsync("roles", role);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View(role);
            }
        }

        // GET: Roles/Edit/5
        public ActionResult Edit(string id)
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

            var responseTask = httpClient.GetAsync("roles/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<IdentityRole>();
                readTask.Wait();

                var roles = readTask.Result;
                return View(roles);
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
                return View(new IdentityRole());

                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id, Name")]IdentityRole role)
        {
            if (role.Name == null) // we add the validation here.
            {
                ModelState.AddModelError("Name", "Name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.PutAsJsonAsync("roles", role);
            responseTask.Wait();

            var response = responseTask.Result;
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
                ModelState.AddModelError("", response.Content.ReadAsStringAsync().Result);
                return View(role);
            }
        }

        // GET: Roles/Delete/5
        public ActionResult Delete(string id)
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

            var responseTask = httpClient.GetAsync("roles/" + id);
            responseTask.Wait();

            var response = responseTask.Result;
            if (response.IsSuccessStatusCode)
            {
                var readTask = response.Content.ReadAsAsync<IdentityRole>();
                readTask.Wait();

                var roles = readTask.Result;
                return View(roles);
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
                return View(new IdentityRole());

                //return (new HttpStatusCodeResult(HttpStatusCode.BadRequest));
            }
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (Request.Cookies["BearerToken"] != null)
            {
                var token = Request.Cookies["BearerToken"].Value;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var responseTask = httpClient.DeleteAsync("roles/" + id);
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
