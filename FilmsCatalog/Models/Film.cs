using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models
{
    public class Film
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public String CreatorId { get; set; }
        public User Creator { get; set; }

        [Required]
        [MaxLength(200)]
        public String Name { get; set; }

        [Required]
        public String Description { get; set; }

        [Required]
        public Int32 ReleaseYear { get; set; } = System.DateTime.Now.Year;

        [Required]
        public String Producer { get; set; }

        [Required]
        public String Path { get; set; }
    }
}
