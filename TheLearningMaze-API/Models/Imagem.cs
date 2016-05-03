using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheLearningMaze_API.Models
{
    [Table("Imagem")]
    public class Imagem
    {
        [Key]
        public Int32 codImagem { get; set; }

        public String tituloImagem { get; set; }

        public Byte[] bitmapImagem { get; set; }
    }
}