using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Status")]
    public class Status
    {
        [Key]
        public String codStatus { get; set; }

        public String descStatus { get; set; }
    }
}