﻿using System.Linq;
using Microsoft.AspNet.SignalR;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Hubs
{
    public class EventoHub : Hub
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public void JoinEvento(int codGrupo = 0, int codParticipante = 0)
        {
            if (codGrupo == 0)
                return;

            var grupo = _db.Grupos.FirstOrDefault(g => g.codGrupo == codGrupo);

            if (grupo == null)
                return;
            
            var evento = _db.Eventos.FirstOrDefault(e => e.codEvento == grupo.codEvento);

            if (evento == null)
                return;

            Groups.Add(Context.ConnectionId, evento.codEvento.ToString());

            // Verificar se codParticipante é líder e guardar na tabela
            if (codParticipante != 0 && codParticipante == grupo.codLider)
            {
                var meo = _db.MasterEventosOrdem.FirstOrDefault(m => m.codGrupo == grupo.codGrupo);

                if (meo != null)
                {
                    meo.codConexao = Context.ConnectionId;
                    _db.Entry(meo).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
            }

            var infoGrupoParticipante = new
            {
                Participante = new
                {
                  codParticipante  
                },
                Grupo = grupo
            };

            Clients.Group(evento.codEvento.ToString()).joinEvento(infoGrupoParticipante);
        }

        public void JoinEventoProfessor(string codEvento)
        {
            Groups.Add(Context.ConnectionId, codEvento);
        }

        public void LancarPergunta(string codEvento, int codQuestao)
        {
            var questao = _db.Questaos.FirstOrDefault(q => q.codQuestao == codQuestao);

            var alternativas = _db.Alternativas.Where(a => a.codQuestao == codQuestao).ToList();

            var assunto = _db.Assuntos.FirstOrDefault(a => a.codAssunto == questao.codAssunto);

            var questaoCompleta = new
            {
                Questao = questao,
                Assunto = assunto,
                Alternativas = alternativas
            };

            Clients.OthersInGroup(codEvento).lancarPergunta(questaoCompleta);
        }

        public void ResponderPergunta(string codEvento, int codGrupo, bool acertou)
        {
            var campeao = false;
            var codGrupoCampeao = 0;
            var acertos = 0;

            if (acertou)
            {
                var questoesAcertadas = _db.QuestaoGrupos.Where(g => g.codGrupo == codGrupo && g.correta).ToList();
                acertos = questoesAcertadas.Count;

                if (acertos > 8)
                {
                    campeao = true;

                    var grupoCampeao = questoesAcertadas.FirstOrDefault();
                    if (grupoCampeao != null)
                        codGrupoCampeao = grupoCampeao.codGrupo;
                }
            }
            Clients.Group(codEvento).responderPergunta(acertou, campeao, codGrupoCampeao, acertos);
        }

        public void AtivarTimer(string codEvento, int tempo)
        {
            Clients.OthersInGroup(codEvento).ativarTimer(tempo);
        }

        public void IniciarJogo(string codEvento)
        {
            Clients.OthersInGroup(codEvento).iniciarJogo();
        }

        public void EncerrarJogo(string codEvento)
        {
            Clients.OthersInGroup(codEvento).encerrarJogo();
        }
    }
}