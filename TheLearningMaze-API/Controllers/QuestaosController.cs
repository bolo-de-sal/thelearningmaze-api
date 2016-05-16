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
    public class QuestaosController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Questaos
        public IQueryable<Questao> GetQuestaos()
        {
            return db.Questaos;
        }

        // GET: api/Questaos/5
        [ResponseType(typeof(Questao))]
        public IHttpActionResult GetQuestao(int id)
        {
            Questao questao = db.Questaos.Find(id);
            if (questao == null)
            {
                return NotFound();
            }

            return Ok(questao);
        }

        // PUT: api/Questaos/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutQuestao(int id, Questao questao, Evento ev)
        {
            Evento evento = db.Eventos.Find(ev.codEvento);
            if (evento.codStatus == "E") && 
            //Caso o status do evento seja “E” e ainda existam questões cadastradas
            //para o evento que não foram respondidas
            {

            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != questao.codQuestao)
            {
                return BadRequest();
            }

            db.Entry(questao).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestaoExists(id))
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

        // POST: api/Questaos
        [ResponseType(typeof(Questao))]
        public IHttpActionResult PostQuestao(Questao questao)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Questaos.Add(questao);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = questao.codQuestao }, questao);
        }

        // DELETE: api/Questaos/5
        [ResponseType(typeof(Questao))]
        public IHttpActionResult DeleteQuestao(int id)
        {
            Questao questao = db.Questaos.Find(id);
            if (questao == null)
            {
                return NotFound();
            }

            db.Questaos.Remove(questao);
            db.SaveChanges();

            return Ok(questao);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool QuestaoExists(int id)
        {
            return db.Questaos.Count(e => e.codQuestao == id) > 0;
        }
    }
}