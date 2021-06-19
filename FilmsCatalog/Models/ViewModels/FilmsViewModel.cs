using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models.ViewModels
{
    public class FilmsViewModel
    {
        [Required(ErrorMessage = "*Это поле является обязательным")]
        [MaxLength(200)]
        public String Name { get; set; }

        [Required(ErrorMessage = "*Это поле является обязательным")]
        public String Description { get; set; }

        [Required(ErrorMessage = "*Это поле является обязательным")]
        public Int32 ReleaseYear { get; set; } = System.DateTime.Now.Year;

        [Required(ErrorMessage = "*Это поле является обязательным")]
        public String Producer { get; set; }
    }
}
