using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("TipoEvento")]
    public class TipoEvento
    {
        [Key]
        public Byte codTipoEvento { get; set; }

        public String descTipoEvento { get; set; }
    }
}