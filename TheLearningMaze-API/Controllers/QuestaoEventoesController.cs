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
    public class QuestaoEventosController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/QuestaoEventos
        public IQueryable<QuestaoEvento> GetQuestaoEventos()
        {
            return db.QuestaoEventos;
        }

        // GET: api/QuestaoEventos/5
        [ResponseType(typeof(QuestaoEvento))]
        public IHttpActionResult GetQuestaoEvento(int id)
        {
            QuestaoEvento questaoEvento = db.QuestaoEventos.Find(id);
            if (questaoEvento == null)
            {
                return NotFound();
            }

            return Ok(questaoEvento);
        }

        // PUT: api/QuestaoEventos/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutQuestaoEvento(int id, QuestaoEvento questaoEvento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != questaoEvento.codEvento)
            {
                return BadRequest();
            }

            db.Entry(questaoEvento).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestaoEventoExists(id))
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

        // POST: api/QuestaoEventos
        [ResponseType(typeof(QuestaoEvento))]
        public IHttpActionResult PostQuestaoEvento(QuestaoEvento questaoEvento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.QuestaoEventos.Add(questaoEvento);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = questaoEvento.codEvento }, questaoEvento);
        }

        // DELETE: api/QuestaoEventos/5
        [ResponseType(typeof(QuestaoEvento))]
        public IHttpActionResult DeleteQuestaoEvento(int id)
        {
            QuestaoEvento questaoEvento = db.QuestaoEventos.Find(id);
            if (questaoEvento == null)
            {
                return NotFound();
            }

            db.QuestaoEventos.Remove(questaoEvento);
            db.SaveChanges();

            return Ok(questaoEvento);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool QuestaoEventoExists(int id)
        {
            return db.QuestaoEventos.Count(e => e.codEvento == id) > 0;
        }
    }
}