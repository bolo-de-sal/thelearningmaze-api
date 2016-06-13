using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    public class Resposta
    {
        public int codEvento { get; set; }

        public string tipoQuestao { get; set; }

        public int alternativa { get; set; }

        public Boolean verdadeiro { get; set; }

        public string texto { get; set; }
    }
}