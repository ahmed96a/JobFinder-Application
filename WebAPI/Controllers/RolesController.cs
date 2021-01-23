using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataAccessLayer;
using Microsoft.AspNet.Identity.EntityFramework;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Roles
        public IHttpActionResult Get()
        {
            try
            {
                return Ok(db.Roles.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Roles/5
        public IHttpActionResult Get(string id)
        {
            try
            {
                var role = db.Roles.Find(id);

                if(role == null)
                {
                    return NotFound();
                }

                return Ok(role);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Roles
        public IHttpActionResult Post([FromBody]IdentityRole role)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    db.Roles.Add(role);
                    db.SaveChanges();
                    return CreatedAtRoute("DefaultApi", new { id = role.Id }, role);
                }

                return BadRequest(ModelState);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Roles/5
        public IHttpActionResult Put([FromBody]AspNetRole role)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var DbRole = db.Roles.Find(role.Id);
                    if(DbRole == null)
                    {
                        return NotFound();
                    }

                    DbRole.Name = role.Name;
                    db.SaveChanges();
                    return Ok(role);
                }

                return BadRequest(ModelState);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Roles/5
        public IHttpActionResult Delete(string id)
        {
            try
            {
                var DbRole = db.Roles.Find(id);
                if(DbRole == null)
                {
                    return NotFound();
                }
                db.Roles.Remove(DbRole);
                db.SaveChanges();
                return Ok();
            }
            catch(Exception ex)
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
