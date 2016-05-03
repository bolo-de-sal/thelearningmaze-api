using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("TipoQuestao")]
    public class TipoQuestao
    {
        [Key]
        public String codTipoQuestao { get; set; }

        public String Descricao { get; set; }
    }
}