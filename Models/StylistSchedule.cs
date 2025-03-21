namespace KlippCoApp.Models;

public class StylistSchedule
{
    public int Id { get; set; }

    public string StylistId { get; set; } // Koppling till frisören (användar-ID)
    public ApplicationUser Stylist { get; set; } // Frisören som använder detta schema
    
    public DateTime Day { get; set; } // Datum för dagen (t.ex. 2025-03-20)
    public TimeSpan StartTime { get; set; } // Starttid för frisörens arbetsdag (t.ex. 08:00)
    public TimeSpan EndTime { get; set; } // Sluttid för frisörens arbetsdag (t.ex. 17:00)
    public TimeSpan BreakStart { get; set; } // Starttid för rast (t.ex. 12:00)
    public TimeSpan BreakEnd { get; set; } // Sluttid för rast (t.ex. 13:00)
    
    public bool IsAvailable { get; set; } // Om frisören är tillgänglig hela dagen eller inte

    public TimeSpan BufferTime { get; set; } = TimeSpan.FromMinutes(15); // Buffer-tid mellan bokningar (t.ex. 15 minuter)
}
