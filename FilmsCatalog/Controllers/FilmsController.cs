using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FilmsCatalog.Data;
using FilmsCatalog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FilmsCatalog.Services;
using FilmsCatalog.Models.ViewModels;
using System.IO;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using PagedList;

namespace FilmsCatalog.Controllers
{
    [Authorize]
    public class FilmsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly ApplicationDbContext context;
        private readonly UserManager<User> userManager;
        private readonly IUserPermissionsService userPermissions;
        private readonly IHostingEnvironment hostingEnvironment;

        public FilmsController(ApplicationDbContext context, UserManager<User> userManager, IUserPermissionsService userPermissions, IHostingEnvironment hostingEnvironment)
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Films
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? page)
        {
            var applicationDbContext = context.Films
                .Include(f => f.Creator);


            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(applicationDbContext.ToPagedList(pageNumber, pageSize));
        }

        // GET: Films/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await context.Films
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        // GET: Films/Create
        public IActionResult Create()
        {

            return this.View(new FilmsCreateModel());
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FilmsCreateModel model)
        {
            var user = await this.userManager.GetUserAsync(this.HttpContext.User);

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Value.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!FilmsController.AllowedExtensions.Contains(fileExt))
            {
                 this.ModelState.AddModelError(nameof(model.File), "Недопустимый формат файла :(");
            }

            if (this.ModelState.IsValid && user != null)
            {
                var film = new Film
                {
                    CreatorId = user.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Producer = model.Producer,
                    ReleaseYear = model.ReleaseYear
                };

                var posterPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + fileExt);
                film.Path = $"/posters/{film.Id:N}{fileExt}";
                using (var fileStream = new FileStream(posterPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }


                this.context.Films.Add(film);
                await context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }
            return this.View(model);
        }

        // GET: Films/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await this.context.Films.SingleOrDefaultAsync(m => m.Id == id);

            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }

            var model = new FilmsViewModel
            {
                Name = film.Name,
                Description = film.Description,
                ReleaseYear = film.ReleaseYear,
                Producer = film.Producer
                
            };

            this.ViewBag.Film = film;
            return this.View(model);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, FilmsViewModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await this.context.Films.SingleOrDefaultAsync(m => m.Id == id);
            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }
            
            if (ModelState.IsValid)
            {
                film.Name = model.Name;
                film.Description = model.Description;
                film.Producer = model.Producer;
                film.ReleaseYear = model.ReleaseYear;

                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", new { id = id});
            }

            

            this.ViewBag.Film = film;
            return this.View(model);
        }

        // GET: Films/EditPoster/5
        public async Task<IActionResult> EditPoster(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await this.context.Films.SingleOrDefaultAsync(m => m.Id == id);

            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }

            var model = new PostersViewModel
            {
                
            };

            this.ViewBag.Film = film;
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPoster(Guid? id, PostersViewModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await this.context.Films.SingleOrDefaultAsync(m => m.Id == id);

            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }

            var posterPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + Path.GetExtension(film.Path));
            System.IO.File.Delete(posterPath);

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Value.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!FilmsController.AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "Недопустимый формат файла :(");
            }

            if (ModelState.IsValid)
            {
                var somePath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + fileExt);
                film.Path = $"/posters/{film.Id:N}{fileExt}";
                using (var fileStream = new FileStream(somePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", new { id = id });
            }

            this.ViewBag.Film = film;
            return this.View(model);
        }

        // GET: Films/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await context.Films
                .Include(f => f.Creator)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }

            return this.View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if(id == null)
            {
                return this.NotFound();
            }

            var film = await this.context.Films
                .SingleOrDefaultAsync(m => m.Id == id);

            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }

            var posterPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + Path.GetExtension(film.Path));
            System.IO.File.Delete(posterPath);

            this.context.Films.Remove(film);
            await this.context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }

        private bool FilmExists(Guid id)
        {
            return context.Films.Any(e => e.Id == id);
        }
    }
}
