using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Questao")]
    public class Questao
    {
        [Key]
        public Int32 codQuestao { get; set; }

        public String textoQuestao { get; set; }

        public Int32 codAssunto { get; set; }

        public Int32? codImagem { get; set; }

        public String codTipoQuestao { get; set; }

        public Int16 codProfessor { get; set; }

        public Boolean ativo { get; set; }

        public String dificuldade { get; set; }

        [NotMapped]
        public int tempo { get; set; }
    }
}