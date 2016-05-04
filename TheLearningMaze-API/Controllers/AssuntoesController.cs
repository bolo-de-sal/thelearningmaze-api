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
    public class AssuntoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Assuntoes
        public IQueryable<Assunto> GetAssuntoes()
        {
            return db.Assuntoes;
        }

        // GET: api/Assuntoes/5
        [ResponseType(typeof(Assunto))]
        public IHttpActionResult GetAssunto(int id)
        {
            Assunto assunto = db.Assuntoes.Find(id);
            if (assunto == null)
            {
                return NotFound();
            }

            return Ok(assunto);
        }

        // PUT: api/Assuntoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutAssunto(int id, Assunto assunto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != assunto.codAssunto)
            {
                return BadRequest();
            }

            db.Entry(assunto).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssuntoExists(id))
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

        // POST: api/Assuntoes
        [ResponseType(typeof(Assunto))]
        public IHttpActionResult PostAssunto(Assunto assunto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Assuntoes.Add(assunto);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = assunto.codAssunto }, assunto);
        }

        // DELETE: api/Assuntoes/5
        [ResponseType(typeof(Assunto))]
        public IHttpActionResult DeleteAssunto(int id)
        {
            Assunto assunto = db.Assuntoes.Find(id);
            if (assunto == null)
            {
                return NotFound();
            }

            db.Assuntoes.Remove(assunto);
            db.SaveChanges();

            return Ok(assunto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool AssuntoExists(int id)
        {
            return db.Assuntoes.Count(e => e.codAssunto == id) > 0;
        }
    }
}