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
        private ApplicationDbContext db = new ApplicationDbContext();

        [ProfAuthFilter]
        // GET: api/Eventos/5/10
        [Route("api/Eventos/Paged/{page}/{perPage}")]
        public IHttpActionResult GetEventosPaginated(int page = 0, int perPage = 10)
        {
            var token = Request.Headers.Authorization.ToString();

            // Faz decode do Token para extrair codProfessor e token original
            var tokenProf = new TokenProf().DecodeToken(token);

            var eventos = db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor
                        && e.codTipoEvento == 4
                        && e.codStatus != "E")
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

        [ProfAuthFilter]
        // GET: api/Eventos/5
        [ResponseType(typeof(Evento))]
        public IHttpActionResult GetEvento(int id)
        {
            var evento = db.Eventos.FirstOrDefault(e => e.codEvento == id);

            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            return Ok(evento);
        }

        // GET: api/Eventos/25/1
        [ResponseType(typeof(Evento))]
        [Route("api/Eventos/{grupoID}/{participanteID}")]
        public IHttpActionResult GetEventoPorGrupo(int grupoID, int participanteID)
        {
            var grupo = db.Grupos.FirstOrDefault(g => g.codGrupo == grupoID);

            if (grupo == null)
                return Content(HttpStatusCode.NotFound, new { message = "Grupo não encontrado" });

            var participante = db.ParticipanteGrupos.FirstOrDefault(p => p.codGrupo == grupoID && p.codParticipante == participanteID);

            if (participante == null)
                return Content(HttpStatusCode.NotFound, new { message = "Participante não encontrado" });

            var evento = db.Eventos.FirstOrDefault(e => e.codEvento == grupo.codEvento);

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
            var evento = db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && (e.codStatus == "E") && e.codTipoEvento == 4)
                .OrderByDescending(d => d.data)
                .FirstOrDefault()
                ?? db.Eventos
                    .Where(e => e.codProfessor == tokenProf.codProfessor && (e.codStatus == "A") && e.codTipoEvento == 4)
                    .OrderByDescending(d => d.data)
                    .FirstOrDefault();

            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            return Ok(evento);
        }

        // GET: api/Eventos/5/Grupos
        [ResponseType(typeof(Grupo))]
        [Route("api/Eventos/{id}/Grupos")]
        public IHttpActionResult GetGruposEvento(int id)
        {
            if (!ValidaProfessor(id)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            var evento = db.Eventos.Find(id);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            IEnumerable<Grupo> grupos = db.Grupos.Where(g => g.codEvento == evento.codEvento);

            if (!grupos.Any())
                return Content(HttpStatusCode.NotFound, new { message = "Evento não tem grupos cadastrados" });

            return Ok(grupos);

        }

        // GET: api/Eventos/GrupoId/25/Grupo
        [ResponseType(typeof(Grupo))]
        [Route("api/Eventos/GrupoId/{id}/Grupos")]
        public IHttpActionResult GetGruposEventoPorGrupoId(int id)
        {
            var eventoId = db.Grupos.Find(id).codEvento;
            if (eventoId == 0) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            IEnumerable<Grupo> grupos = db.Grupos.Where(g => g.codEvento == eventoId);

            if (!grupos.Any())
                return Content(HttpStatusCode.NotFound, new { message = "Evento não tem grupos cadastrados" });

            return Ok(grupos);

        }

        // GET: api/Eventos/5/GruposCompleto
        [Route("api/Eventos/{id}/GruposCompleto")]
        public IHttpActionResult GetGruposFull(int id)
        {
            if (!this.ValidaProfessor(id))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento" });

            var evento = db.Eventos.Find(id);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            var grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .ToList();

            if (grupos.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não tem grupos cadastrados" });

            var retorno = new List<object>();

            foreach (var grupo in grupos)
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

                var assunto = db.Assuntos.FirstOrDefault(a => a.codAssunto == grupo.codAssunto);

                if (assunto == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Grupo não tem assunto definido ou assunto não encontrado" });

                var acertos = db.QuestaoGrupos
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
            if (!this.ValidaProfessor(id)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            var evento = db.Eventos.Find(id);

            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });
            if (evento.codStatus == "A" && evento.codStatus == "C") return Content(HttpStatusCode.BadRequest, new { message = "Evento ainda não foi iniciado" });

            var grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .ToList();

            var retorno = new List<Object>();

            foreach (var grupo in grupos)
            {
                var qg = db.QuestaoGrupos
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
            if (!this.ValidaProfessor(id)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            var assuntosEvento = db.EventoAssuntos
                                        .Where(e => e.codEvento == id)
                                        .ToList();

            if (assuntosEvento.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há assuntos cadastrados para o event!" });

            var retorno = new List<Assunto>();

            foreach (var item in assuntosEvento)
            {
                var assunto = db.Assuntos.Find(item.codAssunto);

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
            if (!this.ValidaProfessor(id)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            var questoes = db.QuestaoEventos
                             .Where(q => q.codEvento == id && q.codStatus != "E")
                             .Select(q => q.codQuestao)
                             .ToList();

            if (questoes.Count <= 0)
            {
                questoes = this.ReciclaQuestoes(id);
                if (questoes.Count <= 0) return Content(HttpStatusCode.NotFound, new { message = "Evento não tem questões cadastradas/para serem recicladas" });
            }


            var retorno = new List<Object>();

            foreach (var qe in questoes)
            {
                var q = db.Questaos.Find(qe);
                if (q == null) return Content(HttpStatusCode.NotFound, new { message = "Questão não encontrada" });
                var a = db.Assuntos.Find(q.codAssunto);
                if (a == null) return Content(HttpStatusCode.NotFound, new { message = "Assunto não encontrado" });
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
            if (!this.ValidaProfessor(eventoID))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento" });

            var questaoEventoAtual = db.QuestaoEventos.FirstOrDefault(q => q.codEvento == eventoID && q.codStatus == "E");

            if (questaoEventoAtual == null || questaoEventoAtual.codQuestao == 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há questão em execução neste evento" });

            var questaoAtual = db.Questaos.FirstOrDefault(q => q.codQuestao == questaoEventoAtual.codQuestao);

            if (questaoAtual == null)
                return Content(HttpStatusCode.NotFound, new { message = "A questão atual não foi encontrada na base de questões" });

            questaoAtual.tempo = questaoAtual.dificuldade.Equals("F") ? 30 : questaoAtual.dificuldade.Equals("M") ? 45 : 60;

            return Ok(questaoAtual);
        }
        
        // GET: api/Eventos/5/QuestaoAtual/Alternativas
        [Route("api/Eventos/{id}/QuestaoAtual/Alternativas")]
        public IHttpActionResult GetQuestaoAtualAlternativas(int id)
        {
            if (!this.ValidaProfessor(id))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento" });

            var questaoSendoRespondida = db.QuestaoEventos.FirstOrDefault(q => q.codEvento == id && q.codStatus == "E");

            if (questaoSendoRespondida == null || questaoSendoRespondida.codQuestao == 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há questão em execução neste evento" });

            var questaoAtual = db.Questaos.FirstOrDefault(q => q.codQuestao == questaoSendoRespondida.codQuestao);

            if (questaoAtual == null)
                return Content(HttpStatusCode.BadRequest, new { message = "Questão atual não encontrada na lista de questões" });

            if (questaoAtual.codTipoQuestao != "A")
                return Content(HttpStatusCode.BadRequest, new { message = "Questão não é de alternativas" });

            var alternativas = db.Alternativas.Where(e => e.codQuestao == questaoAtual.codQuestao).ToList();

            return Ok(alternativas);
        }

        // GET: api/Eventos/5/InfoGrupoAtual
        [Route("api/Eventos/{id}/InfoGrupoAtual")]
        public IHttpActionResult GetInfoGrupoAtual(int id)
        {
            if (!this.ValidaProfessor(id))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

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

            var eventoAssuntos = (from ea in db.EventoAssuntos
                                  join a in db.Assuntos on ea.codAssunto equals a.codAssunto
                                  where ea.codEvento == id
                                  select new
                                  {
                                      a.codAssunto,
                                      a.descricao
                                  }
                                 ).ToList();

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

            var tempoQuestaoAtual = dificuldadeAtual.Equals("F") ? WebConfigurationManager.AppSettings["tempoQuestaoFacil"] : dificuldadeAtual.Equals("M") ? WebConfigurationManager.AppSettings["tempoQuestaoMedia"] : WebConfigurationManager.AppSettings["tempoQuestaoDificil"];

            var indexCodAssuntoAtual = indexCodAssunto + qtdMovimentosAssuntos;

            if (indexCodAssuntoAtual > eventoAssuntos.Count)
            {
                indexCodAssuntoAtual = indexCodAssuntoAtual - eventoAssuntos.Count - 1;
            }

            var codAssuntoAtual = eventoAssuntos[indexCodAssuntoAtual].codAssunto;
            var descricaoAssuntoAtual = eventoAssuntos[indexCodAssuntoAtual].descricao;

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
                                              q.dificuldade,
                                              tempo = tempoQuestaoAtual
                                          }).FirstOrDefault();

            var informacaoAlternativas = from qe in db.QuestaoEventos
                                         join q in db.Questaos on qe.codQuestao equals q.codQuestao
                                         join alternativa in db.Alternativas on q.codQuestao equals alternativa.codQuestao
                                         where qe.codEvento == id && qe.codStatus.Equals("E")
                                         select new
                                         {
                                             alternativa.codAlternativa,
                                             alternativa.textoAlternativa,
                                             alternativa.correta
                                         };

            var informacaoAtual = new
            {
                Grupo = informacaoGrupoAtual,
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
            if (!this.ValidaProfessor(ev.codEvento)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            // Seleciona evento e altera status
            var evento = db.Eventos.Find(ev.codEvento);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });
            if (evento.codStatus == "E" && evento.codStatus == "F") return Content(HttpStatusCode.BadRequest, new { message = "Evento já em execução ou finalizado" });

            // Verificar integridade do evento pra não iniciar malucão
            var quantGrupos = db.Grupos.Count(g => g.codEvento == evento.codEvento);
            if (quantGrupos < 4)
                return Content(HttpStatusCode.BadRequest, new { message = "Evento não tem grupos suficientes!" });

            var quantAssuntosGrupo =
                db.Grupos.Where(g => g.codEvento == evento.codEvento).Select(g => g.codAssunto).Distinct();
            var quantAssuntos =
                db.EventoAssuntos.Where(g => g.codEvento == evento.codEvento).Select(g => g.codAssunto).Distinct();
            if (quantAssuntos.Count() < 4) // Se o evento tem menos de 4 assuntos
                return Content(HttpStatusCode.BadRequest,
                    new { message = "Evento não tem quantidade de assuntos suficientes!" });
            if (quantAssuntos.Count() < quantAssuntosGrupo.Count()) // Se tem mais assuntos escolhidos nos grupos do que disponíveis no evento
                return Content(HttpStatusCode.BadRequest, new { message = "Quantidade de assuntos inconsistente!" });

            evento.codStatus = "E";
            evento.data = DateTime.Now;
            db.Entry(evento).State = EntityState.Modified;

            // Sorteia ordem
            IEnumerable<Grupo> grupos = db.Grupos
                .Where(g => g.codEvento == evento.codEvento)
                .OrderBy(x => Guid.NewGuid());

            if (grupos.Count() < 2) return Content(HttpStatusCode.BadRequest, new { message = "Evento não tem grupos suficientes para iniciar" });

            var retorno = new List<MasterEventosOrdem>();
            Byte i = 0;
            foreach (var grupo in grupos)
            {
                var pg = db.ParticipanteGrupos.FirstOrDefault(p => p.codGrupo == grupo.codGrupo);
                if (pg == null) return Content(HttpStatusCode.BadRequest, new { message = "Grupo não contém participantes" });

                var ordem = new MasterEventosOrdem
                {
                    codGrupo = grupo.codGrupo,
                    ordem = i
                };
                retorno.Add(ordem);
                db.MasterEventosOrdem.Add(ordem);
                i++;
            }

            db.SaveChanges();

            return Ok(retorno);
        }

        [ProfAuthFilter]
        // POST: /api/Eventos/Abrir
        [HttpPost]
        [Route("api/Eventos/Abrir")]
        public IHttpActionResult AbrirEvento(Evento ev)
        {
            if (!this.ValidaProfessor(ev.codEvento)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            // Seleciona evento e altera status
            var evento = db.Eventos.FirstOrDefault(e => e.codEvento == ev.codEvento);

            if (evento == null)
                return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });

            if (evento.codTipoEvento != 4)
                return Content(HttpStatusCode.BadRequest, new { message = "Este evento não faz parte do jogo Learning Maze" });

            if (evento.codStatus != "C")
                return Content(HttpStatusCode.BadRequest, new { message = "Evento já em execução, aberto ou finalizado" });

            var token = Request.Headers.Authorization.ToString();
            // Faz decode do Token para extrair codProfessor e token original
            var tokenProf = new TokenProf().DecodeToken(token);
            var tokenProfessor = db.Tokens.FirstOrDefault(t => t.codProfessor == tokenProf.codProfessor);

            if (tokenProfessor == null)
            {
                return Content(HttpStatusCode.BadRequest, new { message = "Professor não encontrado." });
            }

            var eventosAbertosOuExecucao = db.Eventos
                .Where(e => e.codProfessor == tokenProf.codProfessor && (e.codStatus == "E" || e.codStatus == "A") && e.codTipoEvento == 4)
                .ToList();

            if (eventosAbertosOuExecucao.Count > 0)
            {
                return Content(HttpStatusCode.BadRequest, new { message = "Já existe algum evento aberto ou em execução asssociado a este professor" });
            }

            evento.codStatus = "A";
            evento.data = DateTime.Now;
            db.Entry(evento).State = EntityState.Modified;

            tokenProfessor.codEvento = ev.codEvento;

            db.SaveChanges();

            return Ok();
        }

        [ProfAuthFilter]
        // POST: /api/Eventos/Encerrar
        [HttpPost]
        [Route("api/Eventos/Encerrar")]
        public IHttpActionResult EncerrarEvento(Evento ev)
        {
            if (!this.ValidaProfessor(ev.codEvento)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            //TO-DO
            //encerrar todas as questoes abertas

            // Seleciona evento e altera status
            var evento = db.Eventos.Find(ev.codEvento);
            if (evento == null) return Content(HttpStatusCode.NotFound, new { message = "Evento não encontrado" });
            if (evento.codStatus != "E") return Content(HttpStatusCode.BadRequest, new { message = "Evento já encerrado ou não está em execução" });
            evento.codStatus = "F";
            db.Entry(evento).State = EntityState.Modified;

            var questoes = db.QuestaoEventos
                                .Where(q => q.codEvento == ev.codEvento)
                                .ToList();
            foreach (var qe in questoes) qe.codStatus = "F";

            db.SaveChanges();

            return Ok(evento);
        }

        [ProfAuthFilter]
        // POST: api/Eventos/RegistrarPerguntas
        [HttpPost]
        [Route("api/Eventos/RegistrarPerguntas")]
        public IHttpActionResult RegistrarPerguntas(Evento ev, Questao[] q)
        {
            if (!this.ValidaProfessor(ev.codEvento)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            int? e = db.Eventos.Where(w => w.codEvento == ev.codEvento).Select(w => w.codEvento).FirstOrDefault();
            if (e == null && e == 0) return Content(HttpStatusCode.BadRequest, new { message = "Não foi enviado evento válido" });
            if (q == null) return Content(HttpStatusCode.BadRequest, new { message = "Não foram enviadas questões" });

            foreach (var questao in q)
            {
                var qe = new QuestaoEvento
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

        [ProfAuthFilter]
        // POST: /api/Eventos/LancarPergunta
        [HttpPost]
        [Route("api/Eventos/LancarPergunta")]
        public IHttpActionResult LancarPergunta(QuestaoEvento requestQuestao)
        {
            if (!this.ValidaProfessor(requestQuestao.codEvento)) return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento!" });

            using (var dbContext = new ApplicationDbContext())
            {
                var existeQuestaoEmExecucao = dbContext.QuestaoEventos.Any(q => q.codEvento == requestQuestao.codEvento && q.codStatus.Equals("E"));

                if (existeQuestaoEmExecucao)
                    return Content(HttpStatusCode.NotFound, new { message = "Já existe alguma questão sendo respondida" });

                var questao = dbContext.QuestaoEventos
                    .FirstOrDefault(q => q.codQuestao == requestQuestao.codQuestao && q.codEvento == requestQuestao.codEvento);

                if (questao == null)
                    return Content(HttpStatusCode.NotFound, new { message = "Questão inválida" });

                if (questao.codStatus != "C")
                    return Content(HttpStatusCode.BadRequest, new { message = "Questão já lançada" });

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
        public IHttpActionResult ResponderPergunta(Resposta resposta)
        {
            if (!this.ValidaProfessor(resposta.codEvento))
                return Content(HttpStatusCode.Unauthorized, new { message = "Professor não corresponde ao evento" });

            var questaoAtual = db.QuestaoEventos.FirstOrDefault(q => q.codEvento == resposta.codEvento && q.codStatus == "E");

            if (questaoAtual == null || questaoAtual.codQuestao == 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não há nenhuma questão a ser respondida" });

            var alternativas = db.Alternativas.Where(a => a.codQuestao == questaoAtual.codQuestao).ToList();

            if (alternativas.Count <= 0)
                return Content(HttpStatusCode.NotFound, new { message = "Não foram encontradas alternativas para a resposta a ser respondida" });

            var acertou = false;

            switch (resposta.tipoQuestao)
            {
                case "A":
                    if (alternativas.Any(alternativa => alternativa.correta && (alternativa.codAlternativa == resposta.alternativa)))
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

            var questaoGrupo = db.QuestaoGrupos.FirstOrDefault(q => q.codQuestao == questaoAtual.codQuestao);

            if (questaoGrupo == null)
                return Content(HttpStatusCode.NotFound, new { message = "A questão atual não está associada ao grupo da vez" });

            questaoGrupo.tempo = DateTime.Now;
            questaoGrupo.correta = acertou;

            questaoAtual.codStatus = "F";

            switch (resposta.tipoQuestao)
            {
                case "T":
                    questaoGrupo.textoResp = resposta.texto;
                    break;
                case "V":
                    questaoGrupo.textoResp = resposta.verdadeiro.ToString();
                    break;
                default:
                    questaoGrupo.textoResp = null;
                    break;
            }

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

        private bool EventoExists(int id)
        {
            return db.Eventos.Count(e => e.codEvento == id) > 0;
        }

        private bool ValidaProfessor(int codEvento)
        {
            // Faz decode do Token para extrair codProfessor e token original
            var token = Request.Headers.Authorization.ToString();
            var tokenProf = new TokenProf().DecodeToken(token);

            using (var _db = new ApplicationDbContext())
            {
                int codProfessor = _db.Eventos
                                    .Where(e => e.codEvento == codEvento)
                                    .Select(e => e.codProfessor)
                                    .FirstOrDefault();

                if (tokenProf.codProfessor == codProfessor) return true;
                else return false;
            }
        }

        private List<int> ReciclaQuestoes(int id)
        {
            using (ApplicationDbContext _db = new ApplicationDbContext())
            {
                //select qe.*
                //from questaoevento qe
                //inner join questaogrupo qg on qg.codQuestao = qe.codQuestao
                //where qg.correta = 0
                //    and qe.codStatus = 'F'
                var questaoEventos = (from qe in _db.QuestaoEventos
                                      join qg in _db.QuestaoGrupos on qe.codQuestao equals qg.codQuestao
                                      where qg.correta == false
                                            && qe.codStatus.Equals("F")
                                            && qe.codEvento == id
                                      select new
                                      {
                                          qe.codQuestao
                                      }).ToArray();

                if (questaoEventos.Length <= 0)
                    return null;

                var retorno = new List<int>();

                foreach (var qe in questaoEventos)
                {
                    var questaoEvento = _db.QuestaoEventos.Find(qe);
                    questaoEvento.codStatus = "C";
                    _db.Entry(questaoEvento).State = EntityState.Modified;
                    retorno.Add(Convert.ToInt32(qe));
                }

                _db.SaveChanges();

                return retorno;
            }

        }
    }
}