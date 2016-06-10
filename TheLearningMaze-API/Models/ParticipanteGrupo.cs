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
        [Column(Order = 1)]
        public Int32 codGrupo { get; set; }
        [Key]
        [Column(Order = 2)]
        public Int32 codParticipante { get; set; }

        //public virtual ICollection<Grupo> grupo { get; set; }
        //public virtual ICollection<Participante> participante { get; set; }
    }
}