using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("MasterEventosOrdem")]
    public class MasterEventosOrdem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 codGrupo { get; set; }
        public Byte ordem { get; set; }
    }
}