using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models
{
    public class Poster
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Int32 FilmId { get; set; }
        public Film Film { get; set; }
        [Required]
        public String Path { get; set; }
    }
}
