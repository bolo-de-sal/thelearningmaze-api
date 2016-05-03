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
    public class QuestaoEventoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/QuestaoEventoes
        public IQueryable<QuestaoEvento> GetQuestaoEventoes()
        {
            return db.QuestaoEventoes;
        }

        // GET: api/QuestaoEventoes/5
        [ResponseType(typeof(QuestaoEvento))]
        public IHttpActionResult GetQuestaoEvento(int id)
        {
            QuestaoEvento questaoEvento = db.QuestaoEventoes.Find(id);
            if (questaoEvento == null)
            {
                return NotFound();
            }

            return Ok(questaoEvento);
        }

        // PUT: api/QuestaoEventoes/5
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

        // POST: api/QuestaoEventoes
        [ResponseType(typeof(QuestaoEvento))]
        public IHttpActionResult PostQuestaoEvento(QuestaoEvento questaoEvento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.QuestaoEventoes.Add(questaoEvento);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = questaoEvento.codEvento }, questaoEvento);
        }

        // DELETE: api/QuestaoEventoes/5
        [ResponseType(typeof(QuestaoEvento))]
        public IHttpActionResult DeleteQuestaoEvento(int id)
        {
            QuestaoEvento questaoEvento = db.QuestaoEventoes.Find(id);
            if (questaoEvento == null)
            {
                return NotFound();
            }

            db.QuestaoEventoes.Remove(questaoEvento);
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
            return db.QuestaoEventoes.Count(e => e.codEvento == id) > 0;
        }
    }
}