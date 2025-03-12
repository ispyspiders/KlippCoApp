using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace KlippCoApp.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string? Firstname { get; set; }

    [Required]
    public string? Lastname { get; set; }

    public string ProfileImageUrl { get; set; } = string.Empty;

    [NotMapped] // Lagras inte i db
    public IFormFile? ProfileImageFile { get; set; }
}