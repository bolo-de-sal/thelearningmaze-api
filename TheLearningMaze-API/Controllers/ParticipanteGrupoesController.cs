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
    public class ParticipanteGruposController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/ParticipanteGrupos
        public IQueryable<ParticipanteGrupo> GetParticipanteGrupos()
        {
            return db.ParticipanteGrupos;
        }

        // GET: api/ParticipanteGrupos/5
        [ResponseType(typeof(ParticipanteGrupo))]
        public IHttpActionResult GetParticipanteGrupo(int id)
        {
            ParticipanteGrupo participanteGrupo = db.ParticipanteGrupos.Find(id);
            if (participanteGrupo == null)
            {
                return NotFound();
            }

            return Ok(participanteGrupo);
        }

        // PUT: api/ParticipanteGrupos/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutParticipanteGrupo(int id, ParticipanteGrupo participanteGrupo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != participanteGrupo.codGrupo)
            {
                return BadRequest();
            }

            db.Entry(participanteGrupo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipanteGrupoExists(id))
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

        // POST: api/ParticipanteGrupos
        [ResponseType(typeof(ParticipanteGrupo))]
        public IHttpActionResult PostParticipanteGrupo(ParticipanteGrupo participanteGrupo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ParticipanteGrupos.Add(participanteGrupo);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = participanteGrupo.codGrupo }, participanteGrupo);
        }

        // DELETE: api/ParticipanteGrupos/5
        [ResponseType(typeof(ParticipanteGrupo))]
        public IHttpActionResult DeleteParticipanteGrupo(int id)
        {
            ParticipanteGrupo participanteGrupo = db.ParticipanteGrupos.Find(id);
            if (participanteGrupo == null)
            {
                return NotFound();
            }

            db.ParticipanteGrupos.Remove(participanteGrupo);
            db.SaveChanges();

            return Ok(participanteGrupo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ParticipanteGrupoExists(int id)
        {
            return db.ParticipanteGrupos.Count(e => e.codGrupo == id) > 0;
        }
    }
}