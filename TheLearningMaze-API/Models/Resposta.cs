namespace TheLearningMaze_API.Models
{
    public class Resposta
    {
        public int codEvento { get; set; }

        public string tipoQuestao { get; set; }

        public int alternativa { get; set; }

        public bool verdadeiro { get; set; }

        public string texto { get; set; }

        public bool tempoExpirou { get; set; }
    }
}