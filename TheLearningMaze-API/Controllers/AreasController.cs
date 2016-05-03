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
    public class AreasController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Areas
        public IQueryable<Area> GetAreas()
        {
            return db.Areas;
        }

        // GET: api/Areas/5
        [ResponseType(typeof(Area))]
        public IHttpActionResult GetArea(short id)
        {
            Area area = db.Areas.Find(id);
            if (area == null)
            {
                return NotFound();
            }

            return Ok(area);
        }

        // PUT: api/Areas/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutArea(short id, Area area)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != area.codArea)
            {
                return BadRequest();
            }

            db.Entry(area).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AreaExists(id))
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

        // POST: api/Areas
        [ResponseType(typeof(Area))]
        public IHttpActionResult PostArea(Area area)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Areas.Add(area);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = area.codArea }, area);
        }

        // DELETE: api/Areas/5
        [ResponseType(typeof(Area))]
        public IHttpActionResult DeleteArea(short id)
        {
            Area area = db.Areas.Find(id);
            if (area == null)
            {
                return NotFound();
            }

            db.Areas.Remove(area);
            db.SaveChanges();

            return Ok(area);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool AreaExists(short id)
        {
            return db.Areas.Count(e => e.codArea == id) > 0;
        }
    }
}