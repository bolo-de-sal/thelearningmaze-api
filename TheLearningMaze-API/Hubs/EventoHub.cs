using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Hubs
{
    public class EventoHub : Hub
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public void JoinEvento(string identificador, int? codParticipante)
        {
            if (codParticipante != null)
            {
                // Verificar se codParticipante é líder e guardar na tabela
                Evento evento = db.Eventos
                                    .Where(e => e.identificador == identificador)
                                    .FirstOrDefault();
                Grupo grupo = db.Grupos
                                .Where(g => g.codLider == codParticipante && g.codEvento == evento.codEvento).
                                First();
                if (grupo != null)
                {
                    MasterEventosOrdem meo = db.MasterEventosOrdem
                                                .Where(m => m.codGrupo == grupo.codGrupo)
                                                .FirstOrDefault();
                    meo.codConexao = this.Context.ConnectionId;
                    db.Entry(meo).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                Clients.OthersInGroup(identificador).joinEvento("Líder");
                return;
            }
            this.Groups.Add(this.Context.ConnectionId, identificador);
            Clients.Group(identificador).joinEvento("Aluno");
        }

        public void LancarPergunta(string identificador, string pergunta)
        {
            Clients.OthersInGroup(identificador).lancarPergunta(pergunta);
        }

        public void ResponderPergunta(string identificador)
        {
            Clients.OthersInGroup(identificador).responderPergunta();
        }
    }
}