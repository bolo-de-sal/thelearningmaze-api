using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLearningMaze_API.Models
{
    [Table("EventoAssunto")]
    public class EventoAssunto
    {
        [Key]
        [Column(Order = 1)]
        public Int32 codEvento { get; set; }
        [Key]
        [Column(Order = 2)]
        public Int32 codAssunto { get; set; }
    }
}