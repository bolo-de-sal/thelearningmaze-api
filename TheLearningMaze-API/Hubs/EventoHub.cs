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

        public void JoinEvento(int codGrupo, int? codParticipante)
        {
            var tipoAluno = "Aluno";

            var grupo = db.Grupos.First(g => g.codGrupo == codGrupo);
            var evento = db.Eventos.First(e => e.codEvento == grupo.codEvento);

            this.Groups.Add(this.Context.ConnectionId, evento.identificador);

            // Verificar se codParticipante é líder e guardar na tabela
            if (codParticipante != null && codParticipante == grupo.codLider)
            {
                var meo = db.MasterEventosOrdem.First(m => m.codGrupo == grupo.codGrupo);
                meo.codConexao = this.Context.ConnectionId;
                db.Entry(meo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                tipoAluno = "Líder";
            }

            Clients.Group(evento.identificador).joinEvento(tipoAluno);
        }

        public void LancarPergunta(string identificador, int tempo)
        {
            Clients.OthersInGroup(identificador).lancarPergunta(tempo);
        }

        public void ResponderPergunta(string identificador)
        {
            Clients.OthersInGroup(identificador).responderPergunta();
        }
    }
}