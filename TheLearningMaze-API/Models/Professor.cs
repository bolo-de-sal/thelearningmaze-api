using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLearningMaze_API.Models
{
    [Table("Professor")]
    public class Professor
    {
        [Key]
        public Int16 codProfessor { get; set;  }
        
        public String nome { get; set; }

        public String email { get; set; }

        public Byte[] senha { get; set; }

        public String idSenac { get; set; }

        public String tipo { get; set; }
    }
}