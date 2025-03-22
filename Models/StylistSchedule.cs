using System.ComponentModel.DataAnnotations;

namespace KlippCoApp.Models;

public class StylistSchedule
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Stylist")]
    public string? StylistId { get; set; } // Koppling till frisören (användar-ID)
    public ApplicationUser? Stylist { get; set; } // Frisören som använder detta schema

    [Required]
    [Display(Name = "Datum")]
    public DateTime Day { get; set; } // Datum för dagen (t.ex. 2025-03-20)


    // Schematid
    [Display(Name = "Börjar")]
    public TimeSpan StartTime { get; set; } // Starttid för frisörens arbetsdag (t.ex. 08:00)

    [Display(Name = "Slutar")]
    public TimeSpan EndTime { get; set; } // Sluttid för frisörens arbetsdag (t.ex. 17:00)

    // Rasttid
    [Display(Name = "Rast börjar")]
    public TimeSpan BreakStart { get; set; } // Starttid för rast (t.ex. 12:00)

    [Display(Name = "Rast slutar")]
    public TimeSpan BreakEnd { get; set; } // Sluttid för rast (t.ex. 13:00)

    // Tillgänglighet
    [Display(Name = "Arbetar")]
    public bool IsAvailable { get; set; } = true; // Om frisören är tillgänglig hela dagen eller inte

    // Buffertid
    public TimeSpan BufferTime { get; set; } = TimeSpan.FromMinutes(15); // Buffer-tid mellan bokningar (t.ex. 15 minuter)
}
