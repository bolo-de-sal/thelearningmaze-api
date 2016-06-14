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
using WebGrease.Activities;
using Dapper;

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
            var token = Request.Headers.Authorization.ToString();

            // Faz decode do Token para extrair codProfessor e token original
            var tokenProf = new TokenProf().DecodeToken(token);

            var eventos = db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && e.codTipoEvento == 4 && e.codStatus != "E")
                .OrderByDescending(d => d.data)
                .ToList();

            var totalEventos = eventos.Count();
            var totalPaginas = (int)Math.Ceiling((double)totalEventos / perPage);

            var retorno = eventos
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
            Evento evento = db.Eventos.FirstOrDefault(e => e.codEvento == id);

            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            return Ok(evento);
        }

        // GET: api/Eventos/Ativo
        [ResponseType(typeof(Evento))]
        [Route("api/Eventos/Ativo")]
        public IHttpActionResult GetEventoAtivo()
        {
            var token = Request.Headers.Authorization.ToString();

            // Faz decode do Token para extrair codProfessor e token original
            var tokenProf = new TokenProf().DecodeToken(token);

            var evento = db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && (e.codStatus == "E" || e.codStatus == "A") && e.codTipoEvento == 4)
                .OrderByDescending(d => d.data)
                .FirstOrDefault();

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado." });

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
            var evento = db.Eventos.Find(id);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            List<Grupo> grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .ToList();

            if (grupos.Count <= 0) return Content(HttpStatusCode.NotFound, new { message = "Evento não tem grupos cadastrados" });

            List<Object> retorno = new List<Object>();

            foreach (Grupo grupo in grupos)
            {
                var pgs = db.ParticipanteGrupos
                                                .Where(p => p.codGrupo == grupo.codGrupo)
                                                .ToList();

                if (pgs.Count <= 0) return Content(HttpStatusCode.NotFound, new { message = "Grupo não tem participantes" });

                var participantes = new List<Participante>();

                foreach (var pg in pgs)
                {
                    var p = db.Participantes.Find(pg.codParticipante);

                    if (p == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Participante não encontrado" });

                    participantes.Add(p);
                }

                var assunto = db.Assuntos
                                    .Where(a => a.codAssunto == grupo.codAssunto)
                                    .FirstOrDefault();

                if (assunto == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Grupo não tem assunto definido ou assunto não encontrado" });

                var acertos = db.QuestaoGrupos
                                .Where(a => a.codGrupo == grupo.codGrupo && a.correta)
                                .ToList();

                var grupoFull = new { Grupo = grupo, ParticipantesGrupo = participantes, Assunto = assunto, Acertos = acertos.Count };

                retorno.Add(grupoFull);
            }

            if (retorno.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Ocorreu um erro ao trazer as informações do banco de dados. Por favor, tente novamente." });

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

            foreach (Grupo grupo in grupos)
            {
                List<QuestaoGrupo> qg = db.QuestaoGrupos
                                .Where(q => q.codGrupo == grupo.codGrupo)
                                .OrderBy(q => q.tempo)
                                .ToList();

                retorno.Add(new
                {
                    Grupo = grupo,
                    Questoes = qg
                });
            }

            return Ok(retorno);
        }

        // GET: api/Eventos/5/Assuntos
        [Route("api/Eventos/{id}/Assuntos")]
        public IHttpActionResult GetAssuntos(int id)
        {
            var assuntosEvento = db.EventoAssuntos
                                        .Where(e => e.codEvento == id)
                                        .ToList();

            if (assuntosEvento.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há assuntos cadastrados para o evento!" });

            var retorno = new List<Assunto>();

            foreach (var item in assuntosEvento)
            {
                var assunto = db.Assuntos.Find(item.codAssunto);

                if (assunto == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Assunto não encontrado!" });

                retorno.Add(assunto);
            }

            return Ok(retorno);
        }

        // GET: api/Eventos/5/Questoes
        [Route("api/Eventos/{id}/Questoes")]
        public IHttpActionResult GetQuestoesEvento(int id)
        {
            var questoes = db.QuestaoEventos
                             .Where(q => q.codEvento == id && q.codStatus != "E")
                             .Select(q => q.codQuestao)
                             .ToList();
            if (questoes == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não tem questões cadastradas!" });

            List<Object> retorno = new List<Object>();

            foreach (int qe in questoes)
            {
                Questao q = db.Questaos.Find(qe);
                if (q == null) return Content(HttpStatusCode.NotFound, new { message = "Questão não encontrada!" });
                Assunto a = db.Assuntos.Find(q.codAssunto);
                if (a == null) return Content(HttpStatusCode.NotFound, new { message = "Assunto não encontrado!" });
                retorno.Add(new
                        {
                            Questao = q,
                            Assunto = a
                        }
                    );
            }

            return Ok(retorno);
        }

        // GET: api/Eventos/5/QuestaoAtual
        [Route("api/Eventos/{id}/QuestaoAtual")]
        public IHttpActionResult GetQuestaoAtual(int id)
        {
            int? codQuestaoAtual = db.QuestaoEventos
                                    .Where(q => q.codEvento == id && q.codStatus == "E")
                                    .Select(q => q.codQuestao)
                                    .FirstOrDefault();
            if (codQuestaoAtual == null || codQuestaoAtual == 0) return Content(HttpStatusCode.NotFound, new { message = "Não há questão em execução neste evento!" });

            Questao questao = db.Questaos.FirstOrDefault(q => q.codQuestao == codQuestaoAtual);

            return Ok(questao);
        }

        // GET: api/Eventos/5/QuestaoAtual/Alternativas
        [Route("api/Eventos/{id}/QuestaoAtual/Alternativas")]
        public IHttpActionResult GetQuestaoAtualAlternativas(int id)
        {
            int? codQuestaoAtual = db.QuestaoEventos
                                    .Where(q => q.codEvento == id && q.codStatus == "E")
                                    .Select(q => q.codQuestao)
                                    .FirstOrDefault();
            if (codQuestaoAtual == null && codQuestaoAtual == 0) return Content(HttpStatusCode.NotFound, new { message = "Não há questão em execução neste evento!" });

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

        // GET: api/Eventos/5/InfoGrupoAtual
        [Route("api/Eventos/{id}/InfoGrupoAtual")]
        public IHttpActionResult GetInfoGrupoAtual(int id)
        {
            //SELECT TOP 1 g.nmGrupo, a.descricao, Count(qg.codGrupo) AS Quantidade, ordem FROM Grupo g
            //INNER JOIN MasterEventosOrdem eo ( NOLOCK ) ON eo.codGrupo = g.codGrupo
            //INNER JOIN Assunto a (NOLOCK) ON a.codAssunto = g.codAssunto
            //LEFT JOIN QuestaoGrupo qg ( NOLOCK ) ON qg.codGrupo = g.codEvento
            //WHERE codEvento = 10
            //GROUP BY g.nmGrupo, a.descricao, g.codGrupo, eo.ordem
            //ORDER BY Quantidade, ordem
            var informacaoGrupo = (from g in db.Grupos
                                   join meo in db.MasterEventosOrdem on g.codGrupo equals meo.codGrupo
                                   join a in db.Assuntos on g.codAssunto equals a.codAssunto
                                   join qg in db.QuestaoGrupos on g.codGrupo equals qg.codGrupo into ia
                                   from ia1 in ia.DefaultIfEmpty()
                                   where g.codEvento == id
                                   group ia1 by new { g.codGrupo, g.nmGrupo, a.codAssunto, a.descricao, meo.ordem } into infoAtual
                                   orderby new { quantidade = infoAtual.Count(c => c != null), infoAtual.Key.ordem }
                                   select new
                                   {
                                       infoAtual.Key.codGrupo,
                                       infoAtual.Key.nmGrupo,
                                       assunto = new
                                       {
                                           infoAtual.Key.codAssunto,
                                           infoAtual.Key.descricao
                                       },
                                       questao = new
                                       {
                                           qtdRespondidas = infoAtual.Count(c => c != null),
                                           qtdAcertos = infoAtual.Count(c => c != null && c.correta)
                                       },
                                       infoAtual.Key.ordem
                                   }
                                  ).FirstOrDefault();

            //if (informacaoGrupo == null) return Content(HttpStatusCode.NotFound, new { message = "Nenhum grupo encontrado" });

            var eventoAssuntos = (from ea in db.EventoAssuntos
                                  join a in db.Assuntos on ea.codAssunto equals a.codAssunto
                                  where ea.codEvento == id
                                  select new
                                  {
                                      a.codAssunto,
                                      a.descricao
                                  }
                                 ).ToList();

            var codAssuntoAtual = 0;
            var descricaoAssuntoAtual = string.Empty;
            var qtdMovimentosAssuntos = 0;
            var indexCodAssunto = eventoAssuntos.FindIndex(f => f.codAssunto == informacaoGrupo.assunto.codAssunto);
            var dificuldadeAtual = "F";

            switch (informacaoGrupo.questao.qtdAcertos)
            {
                case 4:
                    qtdMovimentosAssuntos += 1;
                    dificuldadeAtual = "M";
                    break;
                case 5:
                    qtdMovimentosAssuntos += 2;
                    dificuldadeAtual = "M";
                    break;
                case 6:
                    qtdMovimentosAssuntos += 3;
                    dificuldadeAtual = "M";
                    break;
                case 7:
                    qtdMovimentosAssuntos += 4;
                    dificuldadeAtual = "M";
                    break;
                case 8:
                    qtdMovimentosAssuntos += 4;
                    dificuldadeAtual = "D";
                    break;
                default:
                    qtdMovimentosAssuntos = 0;
                    break;
            }

            var indexCodAssuntoAtual = indexCodAssunto + qtdMovimentosAssuntos;

            if (indexCodAssuntoAtual > eventoAssuntos.Count)
            {
                indexCodAssuntoAtual = indexCodAssuntoAtual - eventoAssuntos.Count - 1;
            }

            codAssuntoAtual = eventoAssuntos[indexCodAssuntoAtual].codAssunto;
            descricaoAssuntoAtual = eventoAssuntos[indexCodAssuntoAtual].descricao;

            var informacaoGrupoAtual = new
            {
                informacaoGrupo.codGrupo,
                informacaoGrupo.nmGrupo,
                assunto = new
                {
                    codAssunto = codAssuntoAtual,
                    descricao = descricaoAssuntoAtual
                },
                questao = new
                {
                    informacaoGrupo.questao.qtdRespondidas,
                    informacaoGrupo.questao.qtdAcertos,
                    dificuldade = dificuldadeAtual
                },
                informacaoGrupo.ordem
            };


            //SELECT * FROM QuestaoEvento qe ( NOLOCK )
            //INNER JOIN Questao q ( NOLOCK ) ON qe.codQuestao = q.codQuestao
            //WHERE codEvento = 10 AND codStatus = 'E'
            var informacaoQuestaoAtual = (from qe in db.QuestaoEventos
                                          join q in db.Questaos on qe.codQuestao equals q.codQuestao
                                          join a in db.Assuntos on q.codAssunto equals a.codAssunto
                                          where qe.codEvento == id && qe.codStatus.Equals("E")
                                          select new
                                          {
                                              q.codQuestao,
                                              q.textoQuestao,
                                              assunto = new
                                              {
                                                  a.codAssunto,
                                                  a.descricao
                                              },
                                              q.codTipoQuestao,
                                              q.codImagem,
                                              q.dificuldade
                                          }).FirstOrDefault();

            //if (informacaoQuestaoAtual == null) return Content(HttpStatusCode.NotFound, new { message = "Nenhuma questão encontrada" });

            var informacaoAtual = new
            {
                Grupo = informacaoGrupoAtual,
                Questao = informacaoQuestaoAtual
            };

            return Ok(informacaoAtual);
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
            foreach (Grupo grupo in grupos)
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

        // POST: /api/Eventos/Abrir
        [HttpPost]
        [Route("api/Eventos/Abrir")]
        public IHttpActionResult AbrirEvento(Evento ev)
        {
            // Seleciona evento e altera status
            var evento = db.Eventos.FirstOrDefault(e => e.codEvento == ev.codEvento);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado." });

            if (evento.codStatus != "C")
                return Content(HttpStatusCode.BadRequest, new { message = "Evento já em execução, aberto ou finalizado." });

            var token = Request.Headers.Authorization.ToString();
            // Faz decode do Token para extrair codProfessor e token original
            var tokenProf = new TokenProf().DecodeToken(token);
            var tokenProfessor = db.Tokens.FirstOrDefault(t => t.codProfessor == tokenProf.codProfessor);

            if (tokenProfessor == null)
            {
                return Content(HttpStatusCode.BadRequest, new { message = "Professor não encontrado!" });
            }

            //var eventosAbertosOuExecucao = db.Eventos
            //    .Where(e => e.codProfessor == tokenProf.codProfessor && (e.codStatus == "E" || e.codStatus == "A") && e.codTipoEvento == 4)
            //    .ToList();

            //if (eventosAbertosOuExecucao.Count > 0)
            //{
            //    return Content(HttpStatusCode.BadRequest, new { message = "Professor não encontrado!" });
            //}

            evento.codStatus = "A";
            evento.data = DateTime.Now;
            db.Entry(evento).State = EntityState.Modified;

            tokenProfessor.codEvento = ev.codEvento;

            db.SaveChanges();

            return Ok();
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

        // POST: api/Eventos/RegistrarPerguntas
        [HttpPost]
        [Route("api/Eventos/RegistrarPerguntas")]
        public IHttpActionResult RegistrarPerguntas(Evento ev, Questao[] q)
        {
            int? e = db.Eventos.Where(w => w.codEvento == ev.codEvento).Select(w => w.codEvento).FirstOrDefault();
            if (e == null && e == 0) return Content(HttpStatusCode.BadRequest, new { message = "Não foi enviado evento válido!" });
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

        // POST: /api/Eventos/LancarPergunta
        [HttpPost]
        [Route("api/Eventos/LancarPergunta")]
        public IHttpActionResult LancarPergunta(QuestaoEvento requestQuestao)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                var questao = dbContext.QuestaoEventos
                    .FirstOrDefault(q => q.codQuestao == requestQuestao.codQuestao && q.codEvento == requestQuestao.codEvento);

                if (questao == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Questão inválida!" });

                if (questao.codStatus != "C")
                    return Content(HttpStatusCode.BadRequest, new { message = "Questão já lançada!" });

                const string sql =
@"UPDATE QuestaoEvento SET codStatus = 'E' WHERE codEvento = @codEvento AND codQuestao = @codQuestao";

                try
                {
                    dbContext.Database.Connection.ExecuteScalar<int>(sql, new
                    {
                        requestQuestao.codEvento,
                        requestQuestao.codQuestao
                    });
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

                return Ok();
            }
        }

        //POST: api/Eventos/ResponderPergunta
        [HttpPost]
        [Route("api/Eventos/ResponderPergunta")]
        public IHttpActionResult ResponderPergunta(Resposta r)
        {
            int? codQuestaoAtual = db.QuestaoEventos
                                    .Where(q => q.codEvento == r.codEvento && q.codStatus == "E")
                                    .Select(q => q.codQuestao)
                                    .FirstOrDefault();
            if (codQuestaoAtual == null || codQuestaoAtual == 0) return Content(HttpStatusCode.NotFound, new { message = "Questão não encontrada!" });

            List<Alternativa> alts = db.Alternativas
                                        .Where(a => a.codQuestao == codQuestaoAtual)
                                        .ToList();

            bool acertou = false;

            switch (r.tipoQuestao)
            {
                case "A":
                    foreach (Alternativa alt in alts)
                    {
                        if (alt.correta && (alt.codAlternativa == r.alternativa))
                        {
                            acertou = true;
                            break;
                        }
                    }
                    break;

                case "T":
                    foreach (Alternativa alt in alts)
                    {
                        if (alt.textoAlternativa.Trim() == r.texto.Trim())
                        {
                            acertou = true;
                            break;
                        }
                    }
                    break;

                case "V":
                    if (alts.First().correta == r.verdadeiro) acertou = true;
                    break;

                default:
                    return Content(HttpStatusCode.BadRequest, new { message = "Tipo da questão inválido!" });
            }

            QuestaoGrupo qg = db.QuestaoGrupos
                                .Where(q => q.codQuestao == codQuestaoAtual)
                                .FirstOrDefault();

            if (qg == null) return Content(HttpStatusCode.NotFound, new { message = "QuestãoGrupo não encontrada" });

            qg.tempo = DateTime.Now;
            qg.correta = acertou;
            if (r.tipoQuestao == "T")
                qg.textoResp = r.texto;
            else if (r.tipoQuestao == "V")
                qg.textoResp = r.verdadeiro.ToString();
            else
                qg.textoResp = null;

            db.SaveChanges();

            return Ok(qg);
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