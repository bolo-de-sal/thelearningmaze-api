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
    public class AlternativasController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Alternativas
        public IQueryable<Alternativa> GetAlternativas()
        {
            return db.Alternativas;
        }

        // GET: api/Alternativas/5
        [ResponseType(typeof(Alternativa))]
        public IHttpActionResult GetAlternativa(int id)
        {
            Alternativa alternativa = db.Alternativas.Find(id);
            if (alternativa == null)
            {
                return NotFound();
            }

            return Ok(alternativa);
        }

        // PUT: api/Alternativas/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutAlternativa(int id, Alternativa alternativa)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != alternativa.codQuestao)
            {
                return BadRequest();
            }

            db.Entry(alternativa).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlternativaExists(id))
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

        // POST: api/Alternativas
        [ResponseType(typeof(Alternativa))]
        public IHttpActionResult PostAlternativa(Alternativa alternativa)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Alternativas.Add(alternativa);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = alternativa.codQuestao }, alternativa);
        }

        // DELETE: api/Alternativas/5
        [ResponseType(typeof(Alternativa))]
        public IHttpActionResult DeleteAlternativa(int id)
        {
            Alternativa alternativa = db.Alternativas.Find(id);
            if (alternativa == null)
            {
                return NotFound();
            }

            db.Alternativas.Remove(alternativa);
            db.SaveChanges();

            return Ok(alternativa);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool AlternativaExists(int id)
        {
            return db.Alternativas.Count(e => e.codQuestao == id) > 0;
        }
    }
}