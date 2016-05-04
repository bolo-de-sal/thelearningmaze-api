using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("EventoAssunto")]
    public class EventoAssunto
    {
        [Key]
        public Int32 codEvento { get; set; }

        public Int32 codAssunto { get; set; }
    }
}