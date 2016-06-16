using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Participante")]
    public class Participante
    {
        [Key]
        public Int32 codParticipante { get; set; }

        public String nmParticipante { get; set; }

        public Int32 codCurso { get; set; }

        public String email { get; set; }

        [IgnoreDataMember]
        public Byte[] senha { get; set; }

        public Boolean ativo { get; set; }
    }
}