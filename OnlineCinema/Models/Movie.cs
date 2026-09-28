using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCinema.Models;

public class Movie
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву фільму")]
    [Display(Name = "Назва")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть опис фільму")]
    [Display(Name = "Опис")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть жанр")]
    [Display(Name = "Жанр")]
    public string Genre { get; set; } = string.Empty;

    public string PosterBlobName { get; set; } = string.Empty;

    public string VideoBlobName { get; set; } = string.Empty;

    [NotMapped]
    public IFormFile? PosterFile { get; set; }

    [NotMapped]
    public IFormFile? VideoFile { get; set; }

    [NotMapped]
    public string? PosterUrl { get; set; }

    [NotMapped]
    public string? VideoUrl { get; set; }
}