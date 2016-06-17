using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLearningMaze_API.Models
{
    [Table("Alternativa")]
    public class Alternativa
    {
        [Key]
        [Column(Order = 1)]
        public Int32 codQuestao { get; set; }

        [Key]
        [Column(Order = 2)]
        public Byte codAlternativa { get; set; }

        public String textoAlternativa { get; set; }

        public Boolean correta { get; set; }
    }
}