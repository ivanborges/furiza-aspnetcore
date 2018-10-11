using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class DtoTeste
    {
        [Required]
        public string Teste { get; set; }

        [Required]
        public bool? Teste2 { get; set; }
    }
}