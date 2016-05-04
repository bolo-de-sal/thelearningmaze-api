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
    public class TipoEventoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/TipoEventoes
        public IQueryable<TipoEvento> GetTipoEventoes()
        {
            return db.TipoEventoes;
        }

        // GET: api/TipoEventoes/5
        [ResponseType(typeof(TipoEvento))]
        public IHttpActionResult GetTipoEvento(byte id)
        {
            TipoEvento tipoEvento = db.TipoEventoes.Find(id);
            if (tipoEvento == null)
            {
                return NotFound();
            }

            return Ok(tipoEvento);
        }

        // PUT: api/TipoEventoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTipoEvento(byte id, TipoEvento tipoEvento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tipoEvento.codTipoEvento)
            {
                return BadRequest();
            }

            db.Entry(tipoEvento).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoEventoExists(id))
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

        // POST: api/TipoEventoes
        [ResponseType(typeof(TipoEvento))]
        public IHttpActionResult PostTipoEvento(TipoEvento tipoEvento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.TipoEventoes.Add(tipoEvento);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (TipoEventoExists(tipoEvento.codTipoEvento))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = tipoEvento.codTipoEvento }, tipoEvento);
        }

        // DELETE: api/TipoEventoes/5
        [ResponseType(typeof(TipoEvento))]
        public IHttpActionResult DeleteTipoEvento(byte id)
        {
            TipoEvento tipoEvento = db.TipoEventoes.Find(id);
            if (tipoEvento == null)
            {
                return NotFound();
            }

            db.TipoEventoes.Remove(tipoEvento);
            db.SaveChanges();

            return Ok(tipoEvento);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TipoEventoExists(byte id)
        {
            return db.TipoEventoes.Count(e => e.codTipoEvento == id) > 0;
        }
    }
}