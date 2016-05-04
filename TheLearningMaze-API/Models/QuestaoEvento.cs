using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("QuestaoEvento")]
    public class QuestaoEvento
    {
        [Key]
        public Int32 codEvento { get; set; }

        public Int32 codQuestao { get; set; }

        public String codStatus { get; set; }

        public DateTime tempo { get; set; }
    }
}