using System.Linq;
using Microsoft.AspNet.SignalR;
using TheLearningMaze_API.Models;

namespace TheLearningMaze_API.Hubs
{
    public class EventoHub : Hub
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public void JoinEvento(int codGrupo = 0, int codParticipante = 0)
        {
            if (codGrupo == 0)
                return;

            var tipoAluno = "Aluno";
            var grupo = db.Grupos.FirstOrDefault(g => g.codGrupo == codGrupo);

            if (grupo == null)
                return;

            var evento = db.Eventos.FirstOrDefault(e => e.codEvento == grupo.codEvento);

            if (evento == null)
                return;

            this.Groups.Add(this.Context.ConnectionId, evento.identificador);

            // Verificar se codParticipante é líder e guardar na tabela
            if (codParticipante != 0 && codParticipante == grupo.codLider)
            {
                var meo = db.MasterEventosOrdem.FirstOrDefault(m => m.codGrupo == grupo.codGrupo);

                if (meo == null)
                    return;

                meo.codConexao = this.Context.ConnectionId;
                db.Entry(meo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                tipoAluno = "Líder";
            }

            Clients.Group(evento.identificador).joinEvento(tipoAluno);
        }

        public void JoinEventoProfessor(string identificador)
        {
            Groups.Add(Context.ConnectionId, identificador);
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