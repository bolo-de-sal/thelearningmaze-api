using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Area")]
    public class Area
    {
        [Key]
        public Int16 codArea { get; set; }

        public String descricao { get; set; }
    }
}