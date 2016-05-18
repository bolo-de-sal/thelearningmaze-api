using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace TheLearningMaze_API.Hubs
{
    public class EventoHub : Hub
    {
        public void LancarPergunta(string pergunta)
        {
            Clients.Others.lancarPergunta(pergunta);
        }

        public void ResponderPergunta()
        {
            Clients.Others.responderPergunta();
        }
    }
}