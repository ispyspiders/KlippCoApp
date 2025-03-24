using System.ComponentModel.DataAnnotations;

namespace KlippCoApp.Models;

public class Booking
{
    public int Id { get; set; }

    // Användaren som bokar (kunden)
    [Required]
    public string CustomerId { get; set; }
    public ApplicationUser Customer { get; set; }

    // Stylist
    [Required]
    public string StylistId { get; set; }
    public ApplicationUser Stylist { get; set;}

    // Stylistens schema
    [Required]
    public int StylistScheduleId { get; set; }
    public StylistSchedule StylistSchedule { get; set; }

    // Tjänst som bokas
    [Required]
    public int ServiceId { get; set; }
    public Service Service { get; set; }

    // Tid som bokas (bokad tid)
    [Required]
    public DateTime BookingTime { get; set; }

    // Tid då bokningen sker (timestamp)
    public DateTime BookingTimestamp { get; set; } = DateTime.Now;

}