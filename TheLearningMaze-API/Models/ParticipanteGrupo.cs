using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("ParticipanteGrupo")]
    public class ParticipanteGrupo
    {
        [Key]
        public Int32 codGrupo { get; set; }

        public Int32 codParticipante { get; set; }
    }
}