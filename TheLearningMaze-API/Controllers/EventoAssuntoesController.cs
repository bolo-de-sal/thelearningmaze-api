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
    public class EventoAssuntoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/EventoAssuntoes
        public IQueryable<EventoAssunto> GetEventoAssuntoes()
        {
            return db.EventoAssuntoes;
        }

        // GET: api/EventoAssuntoes/5
        [ResponseType(typeof(EventoAssunto))]
        public IHttpActionResult GetEventoAssunto(int id)
        {
            EventoAssunto eventoAssunto = db.EventoAssuntoes.Find(id);
            if (eventoAssunto == null)
            {
                return NotFound();
            }

            return Ok(eventoAssunto);
        }

        // PUT: api/EventoAssuntoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutEventoAssunto(int id, EventoAssunto eventoAssunto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != eventoAssunto.codEvento)
            {
                return BadRequest();
            }

            db.Entry(eventoAssunto).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventoAssuntoExists(id))
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

        // POST: api/EventoAssuntoes
        [ResponseType(typeof(EventoAssunto))]
        public IHttpActionResult PostEventoAssunto(EventoAssunto eventoAssunto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.EventoAssuntoes.Add(eventoAssunto);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = eventoAssunto.codEvento }, eventoAssunto);
        }

        // DELETE: api/EventoAssuntoes/5
        [ResponseType(typeof(EventoAssunto))]
        public IHttpActionResult DeleteEventoAssunto(int id)
        {
            EventoAssunto eventoAssunto = db.EventoAssuntoes.Find(id);
            if (eventoAssunto == null)
            {
                return NotFound();
            }

            db.EventoAssuntoes.Remove(eventoAssunto);
            db.SaveChanges();

            return Ok(eventoAssunto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EventoAssuntoExists(int id)
        {
            return db.EventoAssuntoes.Count(e => e.codEvento == id) > 0;
        }
    }
}