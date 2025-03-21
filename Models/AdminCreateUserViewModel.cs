using System.ComponentModel.DataAnnotations;

namespace KlippCoApp.Models;

public class AdminCreateUserViewModel
{
    [Required(ErrorMessage = "Vänligen ange ett förnamn")]
    [Display(Name = "Förnamn")]
    public string? Firstname { get; set; }

    [Required(ErrorMessage = "Vänligen ange ett efternamn")]
    [Display(Name = "Efternamn")]
    public string? Lastname { get; set; }

    [Required(ErrorMessage = "Vänligen ange en epost")]
    [EmailAddress]
    [Display(Name = "Epost")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vänligen ange ett lösenord")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Lösenord")]
    public string Password { get; set; }


    [Display(Name = "Roll")]
    [Required(ErrorMessage = "Vänligen ange en roll")]
    public string? Role { get; set; } = "Stylist";
}