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
    public class ParticipantesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Participantes
        public IQueryable<Participante> GetParticipantes()
        {
            return db.Participantes;
        }

        // GET: api/Participantes/5
        [ResponseType(typeof(Participante))]
        public IHttpActionResult GetParticipante(int id)
        {
            Participante participante = db.Participantes.Find(id);
            if (participante == null)
            {
                return NotFound();
            }

            return Ok(participante);
        }

        // PUT: api/Participantes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutParticipante(int id, Participante participante)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != participante.codParticipante)
            {
                return BadRequest();
            }

            db.Entry(participante).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipanteExists(id))
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

        // POST: api/Participantes
        [ResponseType(typeof(Participante))]
        public IHttpActionResult PostParticipante(Participante participante)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Participantes.Add(participante);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = participante.codParticipante }, participante);
        }

        // DELETE: api/Participantes/5
        [ResponseType(typeof(Participante))]
        public IHttpActionResult DeleteParticipante(int id)
        {
            Participante participante = db.Participantes.Find(id);
            if (participante == null)
            {
                return NotFound();
            }

            db.Participantes.Remove(participante);
            db.SaveChanges();

            return Ok(participante);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ParticipanteExists(int id)
        {
            return db.Participantes.Count(e => e.codParticipante == id) > 0;
        }
    }
}