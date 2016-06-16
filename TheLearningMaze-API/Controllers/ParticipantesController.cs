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
using TheLearningMaze_API.Filters;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Controllers
{
    [ApiAuthFilter(true)]
    public class ParticipantesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Participantes/5/QuestaoAtual
        [Route("api/Participantes/{id}/QuestaoAtual")]
        public IHttpActionResult GetQuestaoAtual(int id)
        {
            int? codQuestaoAtual = db.QuestaoEventos
                                    .Where(q => q.codEvento == id && q.codStatus == "E")
                                    .Select(q => q.codQuestao)
                                    .FirstOrDefault();
            if (codQuestaoAtual == 0) return Content(HttpStatusCode.NotFound, new { message = "Não há questão em execução neste evento!" });

            Questao questao = db.Questaos.FirstOrDefault(q => q.codQuestao == codQuestaoAtual);

            return Ok(questao);
        }

        // GET: api/Participantes/5/QuestaoAtual/Alternativas
        [Route("api/Participantes/{id}/QuestaoAtual/Alternativas")]
        public IHttpActionResult GetQuestaoAtualAlternativas(int id)
        {
            int? codQuestaoAtual = db.QuestaoEventos
                                    .Where(q => q.codEvento == id && q.codStatus == "E")
                                    .Select(q => q.codQuestao)
                                    .FirstOrDefault();
            if (codQuestaoAtual == 0) return Content(HttpStatusCode.NotFound, new { message = "Não há questão em execução neste evento!" });

            string tipoQuestao = db.Questaos
                        .Where(q => q.codQuestao == codQuestaoAtual)
                        .Select(q => q.codTipoQuestao)
                        .FirstOrDefault();

            if (tipoQuestao != "A") return Content(HttpStatusCode.BadRequest, new { message = "Questão não é de alternativas!" });

            List<Alternativa> alt = db.Alternativas
                                .Where(e => e.codQuestao == codQuestaoAtual)
                                .ToList();

            return Ok(alt);
        }

        // POST: api/Participantes/RegistrarPerguntas
        [HttpPost]
        [Route("api/Participantes/RegistrarPerguntas")]
        public IHttpActionResult RegistrarPerguntas(Evento ev, Questao[] q)
        {
            int? e = db.Eventos.Where(w => w.codEvento == ev.codEvento).Select(w => w.codEvento).FirstOrDefault();
            if (e == 0) return Content(HttpStatusCode.BadRequest, new { message = "Não foi enviado evento válido!" });
            if (q == null) return Content(HttpStatusCode.BadRequest, new { message = "Não foram enviadas questões!" });

            foreach (Questao questao in q)
            {
                QuestaoEvento qe = new QuestaoEvento
                {
                    codEvento = ev.codEvento,
                    codQuestao = questao.codQuestao,
                    codStatus = "C",
                    tempo = null
                };

                db.QuestaoEventos.Add(qe);
            }

            db.SaveChanges();

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}