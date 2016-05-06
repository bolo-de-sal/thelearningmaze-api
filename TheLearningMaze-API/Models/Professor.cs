using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace TheLearningMaze_API.Models
{
    [Table("Professor")]
    public class Professor
    {
        [Key]
        public Int16 codProfessor { get; set;  }
        
        public String nome { get; set; }

        public String email { get; set; }

        [IgnoreDataMember]
        public Byte[] senha { get; set; }

        public String idSenac { get; set; }

        public String tipo { get; set; }

        //Helper para auth
        [NotMapped]
        public String senhaText { get; set; }
    }
}