using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models.ViewModels
{
    public class PostersViewModel
    {
        [Required(ErrorMessage = "*Это поле является обязательным")]
        public IFormFile File { get; set; }
    }
}
