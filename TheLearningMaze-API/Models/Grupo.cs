using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Grupo")]
    public class Grupo
    {
        [Key]
        public Int32 codGrupo { get; set; }

        public Int32 codEvento { get; set; }

        public String nmGrupo { get; set; }

        public Int32 codAssunto { get; set; }

        public Int32 codLider { get; set; }
    }
}