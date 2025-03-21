using System.ComponentModel.DataAnnotations;

namespace KlippCoApp.Models;

public class Service
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Namn")]
    public string? Name { get; set; }

    [Display(Name = "Beskrivning")]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Pris")]
    public int Price { get; set; } = 0; // Noll som standard

    [Required]
    [Display(Name = "Tidsåtgång (minuter)")]
    public int Duration { get; set; } 
}