using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Alternativa")]
    public class Alternativa
    {
        [Key]
        public Int32 codQuestao { get; set; }

        public Byte codAlternativa { get; set; }

        public String textoAlternativa { get; set; }

        public Boolean correta { get; set; }
    }
}