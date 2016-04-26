using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("MasterEventos")]
    public class Token
    {
        [Key]
        public int codToken { get; set; }
        public Int16? codProfessor { get; set; }
        public string token { get; set; }
        public DateTime concedidoEm { get; set; }
        public DateTime expiraEm { get; set; }
        public int? codEvento { get; set; }
    }
}