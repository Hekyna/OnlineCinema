using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Data;
using OnlineCinema.Models;
using OnlineCinema.Services;

namespace OnlineCinema.Controllers;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AzureBlobService _blobService;

    public MoviesController(
        ApplicationDbContext context,
        AzureBlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }
    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies
            .OrderByDescending(m => m.Id)
            .ToListAsync();

        foreach (var movie in movies)
        {
            if (!string.IsNullOrWhiteSpace(movie.PosterBlobName))
            {
                movie.PosterUrl = _blobService.GenerateReadSasUrl(
                    movie.PosterBlobName,
                    30);
            }
        }

        return View(movies);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> Create(Movie movie)
    {
        if (movie.PosterFile == null || movie.PosterFile.Length == 0)
        {
            ModelState.AddModelError(
                nameof(movie.PosterFile),
                "Оберіть постер.");
        }

        if (movie.VideoFile == null || movie.VideoFile.Length == 0)
        {
            ModelState.AddModelError(
                nameof(movie.VideoFile),
                "Оберіть відео або трейлер.");
        }

        if (!ModelState.IsValid)
        {
            return View(movie);
        }

        string posterBlobName =
            $"posters/{Guid.NewGuid()}{Path.GetExtension(movie.PosterFile!.FileName)}";

        string videoBlobName =
            $"videos/{Guid.NewGuid()}{Path.GetExtension(movie.VideoFile!.FileName)}";

        try
        {
            using (var posterStream = movie.PosterFile.OpenReadStream())
            {
                await _blobService.UploadAsync(
                    posterStream,
                    posterBlobName,
                    movie.PosterFile.ContentType);
            }
            using (var videoStream = movie.VideoFile.OpenReadStream())
            {
                await _blobService.UploadAsync(
                    videoStream,
                    videoBlobName,
                    movie.VideoFile.ContentType);
            }
            movie.PosterBlobName = posterBlobName;
            movie.VideoBlobName = videoBlobName;
            movie.PosterFile = null;
            movie.VideoFile = null;

            _context.Movies.Add(movie);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await _blobService.DeleteAsync(posterBlobName);
            await _blobService.DeleteAsync(videoBlobName);

            ModelState.AddModelError(
                "",
                "Не вдалося завантажити файли в Azure Blob Storage.");

            return View(movie);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(movie.PosterBlobName))
        {
            movie.PosterUrl = _blobService.GenerateReadSasUrl(
                movie.PosterBlobName,
                30);
        }
        if (!string.IsNullOrWhiteSpace(movie.VideoBlobName))
        {
            movie.VideoUrl = _blobService.GenerateReadSasUrl(
                movie.VideoBlobName,
                30);
        }

        return View(movie);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        await _blobService.DeleteAsync(movie.PosterBlobName);

        await _blobService.DeleteAsync(movie.VideoBlobName);

        _context.Movies.Remove(movie);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}