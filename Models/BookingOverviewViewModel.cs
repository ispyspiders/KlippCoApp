using Microsoft.AspNetCore.Mvc.Rendering;

namespace KlippCoApp.Models;

public class BookingOverviewViewModel
{
    public string? SelectedStylistId { get; set; }
    public string? SearchCustomerName { get; set; }
    public DateTime? SelectedDate { get; set; }

    public List<SelectListItem>? Stylists { get; set; }

    public List<BookingRowViewModel>? Bookings { get; set; }
}


public class BookingRowViewModel
{
    public int Id { get; set; }
    public string? StylistName { get; set; }
    public string? CustomerName { get; set; }
    public string? ServiceName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}