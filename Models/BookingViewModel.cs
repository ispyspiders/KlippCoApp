namespace KlippCoApp.Models;

public class BookingViewModel
    {
        // Information om tjänsten och stylisten
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string StylistId { get; set; }
        public string StylistName { get; set; }

        // Den valda bokningstiden (kan vara i ISO-format eller DateTime)
        public DateTime BookingTime { get; set; }

        // För att lagra eventuella felmeddelanden för användarinteraktion
        public string ErrorMessage { get; set; }
    }