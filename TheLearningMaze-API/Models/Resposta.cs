namespace TheLearningMaze_API.Models
{
    public class Resposta
    {
        public int codEvento { get; set; }

        public int codGrupo { get; set; }

        public string tipoQuestao { get; set; }

        public byte alternativa { get; set; }

        public bool verdadeiro { get; set; }

        public string texto { get; set; }

        public bool tempoExpirou { get; set; }
    }
}