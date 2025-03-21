namespace KlippCoApp.Models;

public class UserRolesViewModel
{
    public ApplicationUser User { get; set; }
    public IList<string> Roles { get; set; }
}
