using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Evento")]
    public class Evento
    {
        [Key]
        public Int32 codEvento { get; set; }

        public String descricao { get; set; }

        public DateTime data { get; set; }

        public Byte codTipoEvento { get; set; }

        public String codStatus { get; set; }

        public Int16 codProfessor { get; set; }

        public String identificador { get; set; }
    }
}