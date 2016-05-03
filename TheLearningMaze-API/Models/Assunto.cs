using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Assunto")]
    public class Assunto
    {
        [Key]
        public Int32 codAssunto { get; set; }

        public String descricao { get; set; }

        public Int16 codArea { get; set; }
    }
}