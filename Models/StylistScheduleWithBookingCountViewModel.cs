using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KlippCoApp.Models;

public class StylistScheduleWithBookingCountViewModel
{
    public int Id { get; set; }
    public string StylistName { get; set; }
    public string StylistId { get; set; }
    public DateTime Day { get; set; }

    [Display(Name = "Börjar")]
    public TimeSpan StartTime { get; set; }

    [Display(Name = "Slutar")]
    public TimeSpan EndTime { get; set; }

    public TimeSpan BreakStart { get; set; }

    public TimeSpan BreakEnd { get; set; }

    public TimeSpan BufferTime { get; set; }
    public bool IsAvailable { get; set; }

    // Antal bokningar
    public int BookedCount { get; set; }
}