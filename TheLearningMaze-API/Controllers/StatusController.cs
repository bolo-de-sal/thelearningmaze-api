using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Controllers
{
    public class StatusController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Status
        public IQueryable<Status> GetStatus()
        {
            return db.Status;
        }

        // GET: api/Status/5
        [ResponseType(typeof(Status))]
        public IHttpActionResult GetStatus(string id)
        {
            Status status = db.Status.Find(id);
            if (status == null)
            {
                return NotFound();
            }

            return Ok(status);
        }

        // PUT: api/Status/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutStatus(string id, Status status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != status.codStatus)
            {
                return BadRequest();
            }

            db.Entry(status).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StatusExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Status
        [ResponseType(typeof(Status))]
        public IHttpActionResult PostStatus(Status status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Status.Add(status);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (StatusExists(status.codStatus))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = status.codStatus }, status);
        }

        // DELETE: api/Status/5
        [ResponseType(typeof(Status))]
        public IHttpActionResult DeleteStatus(string id)
        {
            Status status = db.Status.Find(id);
            if (status == null)
            {
                return NotFound();
            }

            db.Status.Remove(status);
            db.SaveChanges();

            return Ok(status);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StatusExists(string id)
        {
            return db.Status.Count(e => e.codStatus == id) > 0;
        }
    }
}