using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace TheLearningMaze_API.Hubs
{
    public class EventoHub : Hub
    {
        public void JoinEvento(string identificador)
        {
            this.Groups.Add(this.Context.ConnectionId, identificador);
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