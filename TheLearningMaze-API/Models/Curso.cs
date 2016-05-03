using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Curso")]
    public class Curso
    {
        [Key]
        public Int32 codCurso { get; set; }

        public String nmCurso { get; set; }
    }
}