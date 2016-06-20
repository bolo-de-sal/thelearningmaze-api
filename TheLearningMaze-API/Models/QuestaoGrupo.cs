using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLearningMaze_API.Models
{
    [Table("QuestaoGrupo")]
    public class QuestaoGrupo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 codQuestao { get; set; }

        public Byte codAlternativa { get; set; }

        public Int32 codGrupo { get; set; }

        public DateTime? tempo { get; set; }

        public String textoResp { get; set; }

        public Boolean correta { get; set; }
    }
}