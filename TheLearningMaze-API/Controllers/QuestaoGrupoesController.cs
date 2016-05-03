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
    public class QuestaoGrupoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/QuestaoGrupoes
        public IQueryable<QuestaoGrupo> GetQuestaoGrupoes()
        {
            return db.QuestaoGrupoes;
        }

        // GET: api/QuestaoGrupoes/5
        [ResponseType(typeof(QuestaoGrupo))]
        public IHttpActionResult GetQuestaoGrupo(int id)
        {
            QuestaoGrupo questaoGrupo = db.QuestaoGrupoes.Find(id);
            if (questaoGrupo == null)
            {
                return NotFound();
            }

            return Ok(questaoGrupo);
        }

        // PUT: api/QuestaoGrupoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutQuestaoGrupo(int id, QuestaoGrupo questaoGrupo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != questaoGrupo.codQuestao)
            {
                return BadRequest();
            }

            db.Entry(questaoGrupo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestaoGrupoExists(id))
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

        // POST: api/QuestaoGrupoes
        [ResponseType(typeof(QuestaoGrupo))]
        public IHttpActionResult PostQuestaoGrupo(QuestaoGrupo questaoGrupo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.QuestaoGrupoes.Add(questaoGrupo);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = questaoGrupo.codQuestao }, questaoGrupo);
        }

        // DELETE: api/QuestaoGrupoes/5
        [ResponseType(typeof(QuestaoGrupo))]
        public IHttpActionResult DeleteQuestaoGrupo(int id)
        {
            QuestaoGrupo questaoGrupo = db.QuestaoGrupoes.Find(id);
            if (questaoGrupo == null)
            {
                return NotFound();
            }

            db.QuestaoGrupoes.Remove(questaoGrupo);
            db.SaveChanges();

            return Ok(questaoGrupo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool QuestaoGrupoExists(int id)
        {
            return db.QuestaoGrupoes.Count(e => e.codQuestao == id) > 0;
        }
    }
}