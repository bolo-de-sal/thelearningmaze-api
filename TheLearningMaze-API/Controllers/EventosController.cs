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
    //[ProfAuthFilter]
    public class EventosController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Eventos/5/10
        [Route("api/Eventos/Paged/{page}/{perPage}")]
        public IHttpActionResult GetEventosPaginated(int page = 0, int perPage = 10)
        {
            string token = Request.Headers.Authorization.ToString();

            // Faz decode do Token para extrair codProfessor e token original
            TokenProf tokenProf = new TokenProf().DecodeToken(token);

            List<Evento> eventos = db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && e.codTipoEvento == 4)
                .OrderByDescending(d => d.data)
                .ToList();
            int totalEventos = eventos.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalEventos / perPage);

            List<Evento> retorno = eventos
                .Skip(perPage * page)
                .Take(perPage)
                .ToList();


            return Ok(new
            {
                TotalEventos = totalEventos,
                TotalPaginas = totalPaginas,
                Eventos = retorno
            });
        }

        // GET: api/Eventos/5
        [ResponseType(typeof(Evento))]
        public IHttpActionResult GetEvento(int id)
        {
            Evento evento = db.Eventos.Find(id);

            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            return Ok(evento);
        }

        // GET: api/Eventos/Ativo
        [ResponseType(typeof(Evento))]
        [Route("api/Eventos/Ativo")]
        public IHttpActionResult GetEventoAtivo()
        {
            string token = Request.Headers.Authorization.ToString();

            // Faz decode do Token para extrair codProfessor e token original
            TokenProf tokenProf = new TokenProf().DecodeToken(token);

            Evento evento = db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && e.codStatus == "E" && e.codTipoEvento == 4)
                .OrderByDescending(d => d.data)
                .FirstOrDefault();

            if (evento == null)
            {
                evento = db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && e.codStatus == "A" && e.codTipoEvento == 4)
                .OrderByDescending(d => d.data)
                .FirstOrDefault();
                
                if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });
            }

            return Ok(evento);
        }

        // GET: api/Eventos/5/Grupos
        [ResponseType(typeof(Grupo))]
        [Route("api/Eventos/{id}/Grupos")]
        public IHttpActionResult GetGruposEvento(int id)
        {
            Evento evento = db.Eventos.Find(id);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            IEnumerable<Grupo> grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento);
            if (grupos == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não tem grupos cadastrados" });

            return Ok(grupos);

        }

        // GET: api/Eventos/5/GruposCompleto
        [Route("api/Eventos/{id}/GruposCompleto")]
        public IHttpActionResult GetGruposFull(int id)
        {
            Evento evento = db.Eventos.Find(id);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            List<Grupo> grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .ToList();
            if (grupos == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não tem grupos cadastrados" });

            List<Object> retorno = new List<Object>();

            foreach(Grupo grupo in grupos)
            {
                List<ParticipanteGrupo> pgs = db.ParticipanteGrupos
                                                .Where(p => p.codGrupo == grupo.codGrupo)
                                                .ToList();
                if (pgs.Count == 0) return Content(HttpStatusCode.NotFound, new { message = "Grupo não tem participantes" });

                List<Participante> participantes = new List<Participante>();

                foreach(ParticipanteGrupo pg in pgs)
                {
                    Participante p = db.Participantes.Find(pg.codParticipante);
                    if (p == null) return Content(HttpStatusCode.NotFound, new { message = "Participante não encontrado" });
                    participantes.Add(p);
                }

                Assunto assunto = db.Assuntos
                                    .Where(a => a.codAssunto == grupo.codAssunto)
                                    .FirstOrDefault();
                if (assunto == null) return Content(HttpStatusCode.NotFound, new { message = "Grupo não tem assunto definido/Assunto não encontrado" });
                var grupoFull = new { Grupo = grupo, ParticipantesGrupo = participantes, Assunto = assunto };
                retorno.Add(grupoFull);
            }

            return Ok(retorno);

        }

        // GET: /api/Eventos/5/Acertos
        [Route("api/Eventos/{id}/Acertos")]
        public IHttpActionResult GetAcertosGrupos(int id)
        {
            Evento evento = db.Eventos.Find(id);

            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });
            if (evento.codStatus == "A" && evento.codStatus == "C") return Content(HttpStatusCode.BadRequest, new { message = "Evento ainda não foi iniciado" });

            List<Grupo> grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .ToList();

            List<Object> retorno = new List<Object>();

            foreach(Grupo grupo in grupos)
            {
                List<QuestaoGrupo> qgs = db.QuestaoGrupos
                                            .Where(qg => qg.codGrupo == grupo.codGrupo)
                                            .ToList();
                int i = 0;
                foreach(QuestaoGrupo qg in qgs)
                {
                    if (qg.correta) i++;
                }
                var acertosGrupo = new { codGrupo = grupo.codGrupo, acertos = i };
                retorno.Add(acertosGrupo);
            }

            return Ok(retorno);
        }

        // GET: api/Eventos/5/Questoes
        [Route("api/Eventos/{id}/Questoes")]
        public IHttpActionResult GetQuestoesEvento(int id)
        {
            var questoes = db.QuestaoEventos
                             .Where(q => q.codEvento == id)
                             .Select(q => q.codQuestao)
                             .ToList();
            if (questoes == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não tem questões cadastradas!" });

            List<Questao> retorno = new List<Questao>();

            foreach(int qe in questoes)
            {
                Questao q = db.Questaos.Find(qe);
                if (q == null) return Content(HttpStatusCode.NotFound, new { message = "Questão não encontrada!" });
                retorno.Add(q);
            }

            return Ok(retorno);
        }

        // POST: /api/Eventos/Iniciar
        [HttpPost]
        [Route("api/Eventos/Iniciar")]
        public IHttpActionResult IniciarEvento(Evento ev)
        {
            // Seleciona evento e altera status
            Evento evento = db.Eventos.Find(ev.codEvento);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });
            if (evento.codStatus == "E" && evento.codStatus == "F") return Content(HttpStatusCode.BadRequest, new { message = "Evento já em execução ou finalizado" });
            evento.codStatus = "E";
            evento.data = DateTime.Now;
            db.Entry(evento).State = EntityState.Modified;

            // Sorteia ordem
            IEnumerable<Grupo> grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .OrderBy(x => Guid.NewGuid());

            if (grupos.Count() < 2) return Content(HttpStatusCode.BadRequest, new { message = "Evento não tem grupos suficientes para iniciar" });

            List<MasterEventosOrdem> retorno = new List<MasterEventosOrdem>();
            Byte i = 0;
            foreach(Grupo grupo in grupos)
            {
                ParticipanteGrupo pg = db.ParticipanteGrupos.Where(p => p.codGrupo == grupo.codGrupo).FirstOrDefault();
                if (pg == null) return Content(HttpStatusCode.BadRequest, new { message = "Grupo não contém participantes" });

                MasterEventosOrdem ordem = new MasterEventosOrdem();
                ordem.codGrupo = grupo.codGrupo;
                ordem.ordem = i;
                retorno.Add(ordem);
                db.MasterEventosOrdem.Add(ordem);
                i++;
            }
            
            db.SaveChanges();

            return Ok(retorno);
        }

        // POST: /api/Eventos/Encerrar
        [HttpPost]
        [Route("api/Eventos/Encerrar")]
        public IHttpActionResult EncerrarEvento(Evento ev)
        {
            // Seleciona evento e altera status
            Evento evento = db.Eventos.Find(ev.codEvento);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });
            if (evento.codStatus != "E") return Content(HttpStatusCode.BadRequest, new { message = "Evento já encerrado ou não está em execução" });
            evento.codStatus = "F";
            db.Entry(evento).State = EntityState.Modified;
            db.SaveChanges();

            return Ok(evento);
        }

        // POST: /api/Eventos/LancarPergunta
        [HttpPost]
        [Route("api/Eventos/LancarPergunta")]
        public IHttpActionResult LancarPergunta(Questao q)
        {
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

        private bool EventoExists(int id)
        {
            return db.Eventos.Count(e => e.codEvento == id) > 0;
        }
    }
}