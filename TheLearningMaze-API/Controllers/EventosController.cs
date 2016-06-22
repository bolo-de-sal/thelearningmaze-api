using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using TheLearningMaze_API.Filters;
using TheLearningMaze_API.Models;
using Dapper;

namespace TheLearningMaze_API.Controllers
{
    public class EventosController : ApiController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        [ProfAuthFilter]
        // GET: api/Eventos/5/10
        [Route("api/Eventos/Paged/{page}/{perPage}")]
        public IHttpActionResult GetEventosPaginated(int page = 0, int perPage = 10)
        {
            var token = Request.Headers.Authorization.ToString();

            // Faz decode do Token para extrair codProfessor e token original
            var tokenProf = new TokenProf().DecodeToken(token);

            var eventos = _db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor
                        && e.codTipoEvento == 4
                        && e.codStatus != "E" && e.codStatus != "A")
                .OrderByDescending(d => d.data)
                .ToList();

            var totalEventos = eventos.Count;
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

        [ProfAuthFilter]
        // GET: api/Eventos/5
        [ResponseType(typeof(Evento))]
        public IHttpActionResult GetEvento(int id)
        {
            var evento = _db.Eventos.FirstOrDefault(e => e.codEvento == id);

            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            return Ok(evento);
        }

        // GET: api/Eventos/25/1
        [ResponseType(typeof(Evento))]
        [Route("api/Eventos/{grupoID}/{participanteID}")]
        public IHttpActionResult GetEventoPorGrupo(int grupoID, int participanteID)
        {
            var grupo = _db.Grupos.FirstOrDefault(g => g.codGrupo == grupoID);

            if (grupo == null)
                return Content(HttpStatusCode.NotFound, new { message = "Grupo não encontrado" });

            var participante = _db.ParticipanteGrupos.FirstOrDefault(p => p.codGrupo == grupoID && p.codParticipante == participanteID);

            if (participante == null)
                return Content(HttpStatusCode.NotFound, new { message = "Participante não encontrado" });

            var evento = _db.Eventos.FirstOrDefault(e => e.codEvento == grupo.codEvento);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            return Ok(evento);
        }

        [ProfAuthFilter]
        // GET: api/Eventos/Ativo
        [ResponseType(typeof(Evento))]
        [Route("api/Eventos/Ativo")]
        public IHttpActionResult GetEventoAtivo()
        {
            var token = Request.Headers.Authorization.ToString();

            // Faz decode do Token para extrair codProfessor e token original
            var tokenProf = new TokenProf().DecodeToken(token);


            // Prioriza evento E
            var evento = _db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && (e.codStatus == "E") && e.codTipoEvento == 4)
                .OrderByDescending(d => d.data)
                .FirstOrDefault()
                ?? _db.Eventos
                    .Where(e => e.codProfessor == tokenProf.codProfessor && (e.codStatus == "A") && e.codTipoEvento == 4)
                    .OrderByDescending(d => d.data)
                    .FirstOrDefault();

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            return Ok(evento);
        }

        // GET: api/Eventos/5/Grupos
        [ResponseType(typeof(Grupo))]
        [Route("api/Eventos/{id}/Grupos")]
        public IHttpActionResult GetGruposEvento(int id)
        {
            var evento = _db.Eventos.Find(id);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            var grupos = _db.Grupos.Where(g => g.codEvento == evento.codEvento).ToList();

            if (!grupos.Any())
                return Content(HttpStatusCode.NotFound, new { message = "Evento não tem grupos cadastrados" });

            return Ok(grupos);

        }

        // GET: api/Eventos/5/GruposCompleto
        [Route("api/Eventos/{id}/GruposCompleto")]
        public IHttpActionResult GetGruposFull(int id)
        {
            var evento = _db.Eventos.Find(id);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            var grupos = _db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .ToList();

            if (grupos.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não tem grupos cadastrados" });

            var retorno = new List<object>();

            foreach (var grupo in grupos)
            {
                var pgs = _db.ParticipanteGrupos
                            .Where(p => p.codGrupo == grupo.codGrupo)
                            .ToList();

                if (pgs.Count <= 0) return Content(HttpStatusCode.NotFound, new { message = "Grupo não tem participantes" });

                var participantes = new List<Participante>();

                foreach (var pg in pgs)
                {
                    var p = _db.Participantes.Find(pg.codParticipante);

                    if (p == null)
                        return Content(HttpStatusCode.NotFound, new { message = "Participante não encontrado" });

                    participantes.Add(p);
                }

                var assunto = _db.Assuntos.FirstOrDefault(a => a.codAssunto == grupo.codAssunto);

                if (assunto == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Grupo não tem assunto definido ou assunto não encontrado" });

                var acertos = _db.QuestaoGrupos
                                .Where(a => a.codGrupo == grupo.codGrupo && a.correta)
                                .ToList();

                var grupoFull = new { Grupo = grupo, ParticipantesGrupo = participantes, Assunto = assunto, Acertos = acertos.Count };

                retorno.Add(grupoFull);
            }

            if (retorno.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Ocorreu um erro ao trazer as informações do banco de dados. Por favor, tente novamente" });

            return Ok(retorno);
        }

        // GET: /api/Eventos/5/GruposQuestoes
        [Route("api/Eventos/{id}/GruposQuestoes")]
        public IHttpActionResult GetGruposQuestoes(int id)
        {
            if (!ValidaProfessor(id))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            var evento = _db.Eventos.Find(id);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            if (evento.codStatus == "A" && evento.codStatus == "C")
                return Content(HttpStatusCode.BadRequest, new { message = "Evento ainda não foi iniciado" });

            var grupos = _db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .ToList();

            var retorno = new List<object>();

            foreach (var grupo in grupos)
            {
                var qg = _db.QuestaoGrupos
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
            if (!ValidaProfessor(id))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            var assuntosEvento = _db.EventoAssuntos
                                        .Where(e => e.codEvento == id)
                                        .ToList();

            if (assuntosEvento.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há assuntos cadastrados para o event!" });

            var retorno = new List<Assunto>();

            foreach (var item in assuntosEvento)
            {
                var assunto = _db.Assuntos.Find(item.codAssunto);

                if (assunto == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Assunto não encontrad!" });

                retorno.Add(assunto);
            }

            return Ok(retorno);
        }

        [ProfAuthFilter]
        // GET: api/Eventos/5/Questoes
        [Route("api/Eventos/{id}/Questoes")]
        public IHttpActionResult GetQuestoesEvento(int id)
        {
            if (!ValidaProfessor(id))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            var informacaoGrupo = (from g in _db.Grupos
                                   join meo in _db.MasterEventosOrdem on g.codGrupo equals meo.codGrupo
                                   join a in _db.Assuntos on g.codAssunto equals a.codAssunto
                                   join qg in _db.QuestaoGrupos on g.codGrupo equals qg.codGrupo into ia
                                   from ia1 in ia.DefaultIfEmpty()
                                   where g.codEvento == id
                                   group ia1 by new { g.codGrupo, g.nmGrupo, g.codLider, a.codAssunto, a.descricao, meo.ordem } into infoAtual
                                   orderby new { quantidade = infoAtual.Count(c => c != null), infoAtual.Key.ordem }
                                   select new
                                   {
                                       infoAtual.Key.codGrupo,
                                       infoAtual.Key.nmGrupo,
                                       infoAtual.Key.codLider,
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
                                  ).ToList();

            if (!informacaoGrupo.Any())
                return Ok(new { success = false, message = "Ocorreu uma falha ao buscar as informações do grupo" });

            var informacaoGrupoAtual = informacaoGrupo.Any(a => a.questao.qtdAcertos == 9) ? informacaoGrupo.OrderByDescending(o => o.questao.qtdAcertos).First() : informacaoGrupo.First();

            var eventoAssuntos = (from ea in _db.EventoAssuntos
                                  join a in _db.Assuntos on ea.codAssunto equals a.codAssunto
                                  where ea.codEvento == id
                                  select new
                                  {
                                      a.codAssunto,
                                      a.descricao
                                  }
                                 ).ToList();

            var qtdMovimentosAssuntos = 0;
            var indexCodAssunto = eventoAssuntos.FindIndex(f => f.codAssunto == informacaoGrupoAtual.assunto.codAssunto);
            var dificuldadeAtual = "F";

            switch (informacaoGrupoAtual.questao.qtdAcertos)
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
                case 9:
                    qtdMovimentosAssuntos += 4;
                    dificuldadeAtual = "D";
                    break;
                default:
                    qtdMovimentosAssuntos = 0;
                    break;
            }

            var tempoQuestaoAtual = dificuldadeAtual.Equals("F") ? Convert.ToInt32(WebConfigurationManager.AppSettings["tempoQuestaoFacil"]) : dificuldadeAtual.Equals("M") ? Convert.ToInt32(WebConfigurationManager.AppSettings["tempoQuestaoMedia"]) : Convert.ToInt32(WebConfigurationManager.AppSettings["tempoQuestaoDificil"]);

            var indexCodAssuntoAtual = indexCodAssunto + qtdMovimentosAssuntos;

            if (indexCodAssuntoAtual > (eventoAssuntos.Count - 1))
            {
                indexCodAssuntoAtual = indexCodAssuntoAtual - eventoAssuntos.Count;
            }

            var codAssuntoAtual = eventoAssuntos[indexCodAssuntoAtual].codAssunto;
            var descricaoAssuntoAtual = eventoAssuntos[indexCodAssuntoAtual].descricao;

            var informacaoGrupoAtualCompleta = new
            {
                informacaoGrupoAtual.codGrupo,
                informacaoGrupoAtual.nmGrupo,
                informacaoGrupoAtual.codLider,
                assunto = new
                {
                    codAssunto = codAssuntoAtual,
                    descricao = descricaoAssuntoAtual
                },
                questao = new
                {
                    informacaoGrupoAtual.questao.qtdRespondidas,
                    informacaoGrupoAtual.questao.qtdAcertos,
                    dificuldade = dificuldadeAtual
                },
                informacaoGrupoAtual.ordem
            };

            var informacaoQuestaoAtual = (from qe in _db.QuestaoEventos
                                          join q in _db.Questaos on qe.codQuestao equals q.codQuestao
                                          join a in _db.Assuntos on q.codAssunto equals a.codAssunto
                                          where qe.codEvento == id && qe.codStatus.Equals("E") && q.codAssunto == codAssuntoAtual
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
                                              q.dificuldade,
                                              tempo = (tempoQuestaoAtual - DbFunctions.DiffSeconds(qe.tempo, DateTime.Now)) > 0 ? (tempoQuestaoAtual - DbFunctions.DiffSeconds(qe.tempo, DateTime.Now)) : 0
                                          }).FirstOrDefault();

            object informacaoAlternativas;

            if (informacaoQuestaoAtual != null)
            {
                informacaoAlternativas = from qe in _db.QuestaoEventos
                                         join q in _db.Questaos on qe.codQuestao equals q.codQuestao
                                         join alternativa in _db.Alternativas on q.codQuestao equals alternativa.codQuestao
                                         where qe.codEvento == id && qe.codStatus.Equals("E")
                                                && qe.codQuestao == informacaoQuestaoAtual.codQuestao
                                         select new
                                         {
                                             alternativa.codAlternativa,
                                             alternativa.textoAlternativa
                                         };
            }
            else
            {
                informacaoAlternativas = null;
            }

            var informacaoAtual = new
            {
                Grupo = informacaoGrupoAtualCompleta,
                Questao = informacaoQuestaoAtual,
                Alternativas = informacaoAlternativas
            };

            var questoesAssuntoAtual = (from q in _db.Questaos
                                        join qe in _db.QuestaoEventos on q.codQuestao equals qe.codQuestao
                                        where qe.codEvento == id
                                            && q.codAssunto == informacaoAtual.Grupo.assunto.codAssunto
                                            && q.dificuldade == informacaoAtual.Grupo.questao.dificuldade
                                            && qe.codStatus != "E"
                                            && qe.codStatus != "F"
                                        select new
                                        {
                                            q.codQuestao
                                        }
                                 ).ToList();

            var questoes = _db.QuestaoEventos
                             .Where(q => q.codEvento == id && q.codStatus != "E" && q.codStatus != "F")
                             .Select(q => q.codQuestao)
                             .ToList();

            if (questoesAssuntoAtual.Count <= 0)
            {
                questoes.AddRange(ReciclaQuestoes(id, informacaoAtual.Grupo.assunto.codAssunto, informacaoAtual.Grupo.questao.dificuldade));

                if (questoes.Count <= 0)
                    return Content(HttpStatusCode.NotFound, new { message = "Evento não tem questões disponíveis para o assunto atual do grupo" });
            }

            var retorno = new List<object>();

            foreach (var qe in questoes)
            {
                var q = _db.Questaos.Find(qe);

                if (q == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Questão não encontrada" });

                var a = _db.Assuntos.Find(q.codAssunto);

                if (a == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Assunto não encontrado" });

                retorno.Add(new
                        {
                            Questao = q,
                            Assunto = a
                        }
                    );
            }

            return Ok(retorno);
        }

        [ProfAuthFilter]
        // GET: api/Eventos/5/QuestaoAtual
        [Route("api/Eventos/{id}/QuestaoAtual")]
        public IHttpActionResult GetQuestaoAtual(int eventoID)
        {
            if (!ValidaProfessor(eventoID))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento" });

            var questaoEventoAtual = _db.QuestaoEventos.FirstOrDefault(q => q.codEvento == eventoID && q.codStatus == "E");

            if (questaoEventoAtual == null || questaoEventoAtual.codQuestao == 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há questão em execução neste evento" });

            var questaoAtual = _db.Questaos.FirstOrDefault(q => q.codQuestao == questaoEventoAtual.codQuestao);

            if (questaoAtual == null)
                return Content(HttpStatusCode.NotFound, new { message = "A questão atual não foi encontrada na base de questões" });

            questaoAtual.tempo = questaoAtual.dificuldade.Equals("F") ? 30 : questaoAtual.dificuldade.Equals("M") ? 45 : 60;

            return Ok(questaoAtual);
        }

        // GET: api/Eventos/5/QuestaoAtual/Alternativas
        [Route("api/Eventos/{id}/QuestaoAtual/Alternativas")]
        public IHttpActionResult GetQuestaoAtualAlternativas(int id)
        {
            if (!ValidaProfessor(id))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento" });

            var questaoSendoRespondida = _db.QuestaoEventos.FirstOrDefault(q => q.codEvento == id && q.codStatus == "E");

            if (questaoSendoRespondida == null || questaoSendoRespondida.codQuestao == 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há questão em execução neste evento" });

            var questaoAtual = _db.Questaos.FirstOrDefault(q => q.codQuestao == questaoSendoRespondida.codQuestao);

            if (questaoAtual == null)
                return Content(HttpStatusCode.BadRequest, new { message = "Questão atual não encontrada na lista de questões" });

            if (questaoAtual.codTipoQuestao != "A")
                return Content(HttpStatusCode.BadRequest, new { message = "Questão não é de alternativas" });

            var alternativas = _db.Alternativas.Where(e => e.codQuestao == questaoAtual.codQuestao).ToList();

            return Ok(alternativas);
        }

        // GET: api/Eventos/5/InfoGrupoAtual
        [Route("api/Eventos/{id}/InfoGrupoAtual")]
        public IHttpActionResult GetInfoGrupoAtual(int id)
        {
            var informacaoGrupo = (from g in _db.Grupos
                                   join meo in _db.MasterEventosOrdem on g.codGrupo equals meo.codGrupo
                                   join a in _db.Assuntos on g.codAssunto equals a.codAssunto
                                   join qg in _db.QuestaoGrupos on g.codGrupo equals qg.codGrupo into ia
                                   from ia1 in ia.DefaultIfEmpty()
                                   where g.codEvento == id
                                   group ia1 by new { g.codGrupo, g.nmGrupo, g.codLider, a.codAssunto, a.descricao, meo.ordem } into infoAtual
                                   orderby new { quantidade = infoAtual.Count(c => c != null), infoAtual.Key.ordem }
                                   select new
                                   {
                                       infoAtual.Key.codGrupo,
                                       infoAtual.Key.nmGrupo,
                                       infoAtual.Key.codLider,
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
                                  ).ToList();

            if (!informacaoGrupo.Any())
                return Ok(new { success = false, message = "Ocorreu uma falha ao buscar as informações do grupo" });

            var informacaoGrupoAtual = informacaoGrupo.Any(a => a.questao.qtdAcertos == 9) ? informacaoGrupo.OrderByDescending(o => o.questao.qtdAcertos).First() : informacaoGrupo.First();

            var eventoAssuntos = (from ea in _db.EventoAssuntos
                                  join a in _db.Assuntos on ea.codAssunto equals a.codAssunto
                                  where ea.codEvento == id
                                  select new
                                  {
                                      a.codAssunto,
                                      a.descricao
                                  }
                                 ).ToList();

            var qtdMovimentosAssuntos = 0;
            var indexCodAssunto = eventoAssuntos.FindIndex(f => f.codAssunto == informacaoGrupoAtual.assunto.codAssunto);
            var dificuldadeAtual = "F";

            switch (informacaoGrupoAtual.questao.qtdAcertos)
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
                case 9:
                    qtdMovimentosAssuntos += 4;
                    dificuldadeAtual = "D";
                    break;
                default:
                    qtdMovimentosAssuntos = 0;
                    break;
            }

            var tempoQuestaoAtual = dificuldadeAtual.Equals("F") ? Convert.ToInt32(WebConfigurationManager.AppSettings["tempoQuestaoFacil"]) : dificuldadeAtual.Equals("M") ? Convert.ToInt32(WebConfigurationManager.AppSettings["tempoQuestaoMedia"]) : Convert.ToInt32(WebConfigurationManager.AppSettings["tempoQuestaoDificil"]);

            var indexCodAssuntoAtual = indexCodAssunto + qtdMovimentosAssuntos;

            if (indexCodAssuntoAtual > (eventoAssuntos.Count - 1))
            {
                indexCodAssuntoAtual = indexCodAssuntoAtual - eventoAssuntos.Count;
            }

            var codAssuntoAtual = eventoAssuntos[indexCodAssuntoAtual].codAssunto;
            var descricaoAssuntoAtual = eventoAssuntos[indexCodAssuntoAtual].descricao;

            var informacaoGrupoAtualCompleta = new
            {
                informacaoGrupoAtual.codGrupo,
                informacaoGrupoAtual.nmGrupo,
                informacaoGrupoAtual.codLider,
                assunto = new
                {
                    codAssunto = codAssuntoAtual,
                    descricao = descricaoAssuntoAtual
                },
                questao = new
                {
                    informacaoGrupoAtual.questao.qtdRespondidas,
                    informacaoGrupoAtual.questao.qtdAcertos,
                    dificuldade = dificuldadeAtual
                },
                informacaoGrupoAtual.ordem
            };

            var informacaoQuestaoAtual = (from qe in _db.QuestaoEventos
                                          join q in _db.Questaos on qe.codQuestao equals q.codQuestao
                                          join a in _db.Assuntos on q.codAssunto equals a.codAssunto
                                          where qe.codEvento == id && qe.codStatus.Equals("E") && q.codAssunto == codAssuntoAtual
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
                                              q.dificuldade,
                                              tempo = (tempoQuestaoAtual - DbFunctions.DiffSeconds(qe.tempo, DateTime.Now)) > 0 ? (tempoQuestaoAtual - DbFunctions.DiffSeconds(qe.tempo, DateTime.Now)) : 0
                                          }).FirstOrDefault();

            object informacaoAlternativas;

            if (informacaoQuestaoAtual != null)
            {
                informacaoAlternativas = from qe in _db.QuestaoEventos
                                         join q in _db.Questaos on qe.codQuestao equals q.codQuestao
                                         join alternativa in _db.Alternativas on q.codQuestao equals alternativa.codQuestao
                                         where qe.codEvento == id && qe.codStatus.Equals("E")
                                                && qe.codQuestao == informacaoQuestaoAtual.codQuestao
                                         select new
                                         {
                                             alternativa.codAlternativa,
                                             alternativa.textoAlternativa
                                         };
            }
            else
            {
                informacaoAlternativas = null;
            }

            var informacaoAtual = new
            {
                Grupo = informacaoGrupoAtualCompleta,
                Questao = informacaoQuestaoAtual,
                Alternativas = informacaoAlternativas
            };

            return Ok(informacaoAtual);
        }

        [ProfAuthFilter]
        // POST: /api/Eventos/Iniciar
        [HttpPost]
        [Route("api/Eventos/Iniciar")]
        public IHttpActionResult IniciarEvento(Evento ev)
        {
            if (!ValidaProfessor(ev.codEvento))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            // Seleciona evento e altera status
            var evento = _db.Eventos.Find(ev.codEvento);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            if (evento.codStatus == "E" && evento.codStatus == "F")
                return Content(HttpStatusCode.BadRequest, new { message = "Evento já em execução ou finalizado" });

            var grupos = _db.Grupos.Where(g => g.codEvento == evento.codEvento).OrderBy(x => Guid.NewGuid()).ToList();

            if (grupos.Count < 2)
                return Content(HttpStatusCode.BadRequest, new { message = "Evento não tem grupos suficientes para iniciar" });

            foreach (var grupo in grupos)
            {
                if (!grupo.finalizado.Value)
                    return Content(HttpStatusCode.BadRequest, new { message = "Evento não pode ser iniciado até que todos os grupos estejam finalizados" });
            }

            var quantAssuntosGrupo = _db.Grupos.Where(g => g.codEvento == evento.codEvento).Select(g => g.codAssunto).Distinct();

            var quantAssuntos = _db.EventoAssuntos.Where(g => g.codEvento == evento.codEvento).Select(g => g.codAssunto).Distinct();

            if (quantAssuntos.Count() < 4) // Se o evento tem menos de 4 assuntos
                return Content(HttpStatusCode.BadRequest, new { message = "Evento não tem quantidade de assuntos suficientes!" });

            if (quantAssuntos.Count() < quantAssuntosGrupo.Count()) // Se tem mais assuntos escolhidos nos grupos do que disponíveis no evento
                return Content(HttpStatusCode.BadRequest, new { message = "Quantidade de assuntos inconsistente!" });

            evento.codStatus = "E";
            evento.data = DateTime.Now;
            _db.Entry(evento).State = EntityState.Modified;

            // Sorteia ordem
            var retorno = new List<MasterEventosOrdem>();
            byte i = 0;

            foreach (var grupo in grupos)
            {
                var pg = _db.ParticipanteGrupos.Where(p => p.codGrupo == grupo.codGrupo).ToList();

                if (pg.Count <= 0)
                    return Content(HttpStatusCode.BadRequest, new { message = "Grupo não contém participantes" });

                if (pg.All(p => p.codParticipante != grupo.codLider))
                    return Content(HttpStatusCode.BadRequest, new { message = string.Format("O grupo {0} não contém um líder", grupo.nmGrupo) });

                var grupoJaSorteado = _db.MasterEventosOrdem.Any(g => g.codGrupo == grupo.codGrupo);

                if (!grupoJaSorteado)
                {
                    var ordem = new MasterEventosOrdem
                    {
                        codGrupo = grupo.codGrupo,
                        ordem = i
                    };

                    retorno.Add(ordem);
                    _db.MasterEventosOrdem.Add(ordem);
                }

                i++;
            }

            _db.SaveChanges();

            return Ok(retorno);
        }

        [ProfAuthFilter]
        // POST: /api/Eventos/Abrir
        [HttpPost]
        [Route("api/Eventos/Abrir")]
        public IHttpActionResult AbrirEvento(Evento ev)
        {
            if (!ValidaProfessor(ev.codEvento))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            // Seleciona evento e altera status
            var evento = _db.Eventos.FirstOrDefault(e => e.codEvento == ev.codEvento);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            if (evento.codTipoEvento != 4)
                return Content(HttpStatusCode.BadRequest, new { message = "Este evento não faz parte do jogo Learning Maze" });

            if (evento.codStatus != "C")
                return Content(HttpStatusCode.BadRequest, new { message = "Evento já em execução, aberto ou finalizado" });

            var token = Request.Headers.Authorization.ToString();
            // Faz decode do Token para extrair codProfessor e token original
            var tokenProf = new TokenProf().DecodeToken(token);
            var tokenProfessor = _db.Tokens.FirstOrDefault(t => t.codProfessor == tokenProf.codProfessor);

            if (tokenProfessor == null)
            {
                return Content(HttpStatusCode.BadRequest, new { message = "Professor não encontrado." });
            }

            var eventosAbertosOuExecucao = _db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && (e.codStatus == "E" || e.codStatus == "A") && e.codTipoEvento == 4)
                .ToList();

            if (eventosAbertosOuExecucao.Count > 0)
            {
                return Content(HttpStatusCode.BadRequest, new { message = "Já existe algum evento aberto ou em execução asssociado a este professor" });
            }

            evento.codStatus = "A";
            evento.data = DateTime.Now;
            _db.Entry(evento).State = EntityState.Modified;

            tokenProfessor.codEvento = ev.codEvento;

            _db.SaveChanges();

            return Ok();
        }

        [ProfAuthFilter]
        // POST: /api/Eventos/Encerrar
        [HttpPost]
        [Route("api/Eventos/Encerrar")]
        public IHttpActionResult EncerrarEvento(Evento ev)
        {
            if (!ValidaProfessor(ev.codEvento))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            //TO-DO
            //encerrar todas as questoes abertas

            // Seleciona evento e altera status
            var evento = _db.Eventos.Find(ev.codEvento);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });
            if (evento.codStatus != "E") return Content(HttpStatusCode.BadRequest, new { message = "Evento já encerrado ou não está em execução" });
            evento.codStatus = "F";
            _db.Entry(evento).State = EntityState.Modified;

            var questoes = _db.QuestaoEventos
                                .Where(q => q.codEvento == ev.codEvento)
                                .ToList();
            foreach (var qe in questoes) qe.codStatus = "F";

            _db.SaveChanges();

            return Ok(evento);
        }

        [ProfAuthFilter]
        // POST: api/Eventos/RegistrarPerguntas
        [HttpPost]
        [Route("api/Eventos/RegistrarPerguntas")]
        public IHttpActionResult RegistrarPerguntas(Evento ev, Questao[] questoes)
        {
            if (!ValidaProfessor(ev.codEvento))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            var evento = _db.Eventos.FirstOrDefault(w => w.codEvento == ev.codEvento);

            if (evento == null || evento.codEvento == 0)
                return Content(HttpStatusCode.BadRequest, new { message = "Não foi enviado evento válido" });

            if (questoes == null)
                return Content(HttpStatusCode.BadRequest, new { message = "Não foram enviadas questões" });

            foreach (var questao in questoes)
            {
                var qe = new QuestaoEvento
                                                    {
                                                        codEvento = ev.codEvento,
                                                        codQuestao = questao.codQuestao,
                                                        codStatus = "C",
                                                        tempo = null
                                                    };

                _db.QuestaoEventos.Add(qe);
            }

            _db.SaveChanges();

            return Ok();
        }

        [ProfAuthFilter]
        // POST: /api/Eventos/LancarPergunta
        [HttpPost]
        [Route("api/Eventos/LancarPergunta")]
        public IHttpActionResult LancarPergunta(QuestaoEvento requestQuestao)
        {
            if (!ValidaProfessor(requestQuestao.codEvento))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            using (var dbContext = new ApplicationDbContext())
            {
                var existeQuestaoEmExecucao = dbContext.QuestaoEventos.Any(q => q.codEvento == requestQuestao.codEvento && q.codStatus.Equals("E"));

                if (existeQuestaoEmExecucao)
                    return Content(HttpStatusCode.NotFound, new { message = "Já existe alguma questão sendo respondida" });

                var questao = dbContext.QuestaoEventos
                    .FirstOrDefault(q => q.codQuestao == requestQuestao.codQuestao && q.codEvento == requestQuestao.codEvento);

                if (questao == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Questão inválida" });

                //if (questao.codStatus != "C")
                //return Content(HttpStatusCode.BadRequest, new { message = "Questão já lançada" });

                const string sql =
@"UPDATE QuestaoEvento SET codStatus = 'E', tempo = @tempo WHERE codEvento = @codEvento AND codQuestao = @codQuestao";

                try
                {
                    dbContext.Database.Connection.ExecuteScalar<int>(sql, new
                    {
                        tempo = DateTime.Now,
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
        public IHttpActionResult ResponderPergunta(Resposta resposta)
        {
            var questaoAtual = _db.QuestaoEventos.FirstOrDefault(q => q.codEvento == resposta.codEvento && q.codStatus == "E");

            if (questaoAtual == null || questaoAtual.codQuestao == 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há nenhuma questão a ser respondida" });

            var alternativas = _db.Alternativas.Where(a => a.codQuestao == questaoAtual.codQuestao).ToList();

            if (alternativas.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não foram encontradas alternativas para a resposta a ser respondida" });

            var mesmaResposta =
                _db.QuestaoGrupos.Any(
                    q =>
                        q.codQuestao == questaoAtual.codQuestao
                        && q.codGrupo == resposta.codGrupo
                        && q.codAlternativa == resposta.alternativa);

            if (mesmaResposta)
                return Content(HttpStatusCode.BadRequest, new { message = "Assim como Homer Simpson, você está cometendo o mesmo erro esperando um resultado diferente" });

            var acertou = false;

            if (!resposta.tempoExpirou)
            {
                switch (resposta.tipoQuestao)
                {
                    case "A":
                        if (
                            alternativas.Any(
                                alternativa =>
                                    alternativa.correta && (alternativa.codAlternativa == resposta.alternativa)))
                            acertou = true;

                        break;

                    case "T":
                        if (alternativas.Any(alternativa => alternativa.textoAlternativa.Trim() == resposta.texto.Trim()))
                            acertou = true;

                        break;

                    case "V":
                        if (alternativas.First().correta == resposta.verdadeiro)
                            acertou = true;

                        break;

                    default:
                        return Content(HttpStatusCode.BadRequest, new { message = "Tipo da questão inválido" });
                }
            }
            else
            {
                //var alternativa = _db.Alternativas.First(a => !a.correta);
                var alternativa = _db.Alternativas.Where(a => a.codQuestao == questaoAtual.codQuestao && !a.correta);
                var acabouQuestoesErradas = true;

                foreach (var item in alternativa)
                {
                    var jaEscolheuEssaAlternativa = _db.QuestaoGrupos.Any(q => q.codQuestao == questaoAtual.codQuestao
                                                                               && q.codGrupo == resposta.codGrupo
                                                                               && q.codAlternativa == item.codAlternativa);
                    if (jaEscolheuEssaAlternativa)
                        continue;

                    resposta.alternativa = item.codAlternativa;
                    acabouQuestoesErradas = false;
                    break;
                }

                if (acabouQuestoesErradas)
                {
                    var alternativaCorreta = _db.Alternativas.FirstOrDefault(a => a.codQuestao == questaoAtual.codQuestao && a.correta);

                    if (alternativaCorreta != null)
                        resposta.alternativa = alternativaCorreta.codAlternativa;
                    else
                        return Content(HttpStatusCode.NotFound, new { message = "Não foi encontrada alternativa correta para essa questão" });
                }
            }

            var questaoGrupo = new QuestaoGrupo
            {
                codQuestao = questaoAtual.codQuestao,
                codGrupo = resposta.codGrupo,
                codAlternativa = resposta.alternativa,
                tempo = DateTime.Now,
                correta = acertou
            };

            questaoAtual.codStatus = "F";

            switch (resposta.tipoQuestao)
            {
                case "T":
                    questaoGrupo.codAlternativa = alternativas.First().codAlternativa;
                    questaoGrupo.textoResp = resposta.texto;
                    break;
                case "V":
                    questaoGrupo.codAlternativa = alternativas.First().codAlternativa;
                    questaoGrupo.textoResp = resposta.verdadeiro ? "V" : "F";
                    break;
                default:
                    questaoGrupo.textoResp = string.Empty;
                    break;
            }

            _db.QuestaoGrupos.Add(questaoGrupo);

            _db.SaveChanges();

            return Ok(questaoGrupo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ValidaProfessor(int codEvento)
        {
            // Faz decode do Token para extrair codProfessor e token original
            var token = Request.Headers.Authorization.ToString();
            var tokenProf = new TokenProf().DecodeToken(token);

            using (var dbContext = new ApplicationDbContext())
            {
                int codProfessor = dbContext.Eventos
                                    .Where(e => e.codEvento == codEvento)
                                    .Select(e => e.codProfessor)
                                    .FirstOrDefault();

                return tokenProf.codProfessor == codProfessor;
            }
        }

        private List<int> ReciclaQuestoes(int eventoID, int assuntoID, string dificuldadeAtual)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                var questaoEventos = (from qe in dbContext.QuestaoEventos
                                      join qg in dbContext.QuestaoGrupos on qe.codQuestao equals qg.codQuestao
                                      join q in dbContext.Questaos on qe.codQuestao equals q.codQuestao
                                      where qg.correta == false
                                            && qe.codStatus.Equals("F")
                                            && qe.codEvento == eventoID
                                            && q.codAssunto == assuntoID
                                            && q.dificuldade == dificuldadeAtual
                                      select new
                                      {
                                          qe.codQuestao,
                                          qe.codEvento
                                      }).ToArray();

                if (questaoEventos.Length <= 0)
                    return null;

                var retorno = new List<int>();

                foreach (var qe in questaoEventos)
                {
                    var questaoEvento = dbContext.QuestaoEventos.FirstOrDefault(q => q.codQuestao == qe.codQuestao && q.codEvento == qe.codEvento);

                    if (questaoEvento == null)
                        continue;

                    questaoEvento.codStatus = "C";
                    dbContext.Entry(questaoEvento).State = EntityState.Modified;
                    retorno.Add(Convert.ToInt32(qe.codQuestao));
                }

                dbContext.SaveChanges();

                return retorno;
            }

        }
    }
}