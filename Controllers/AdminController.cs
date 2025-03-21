using KlippCoApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KlippCoApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Lista användare med filtrering och sökning
    [HttpGet]
    public async Task<IActionResult> Index(string roleFilter, string searchString)
    {
        // Hämta alla användare
        var users = _userManager.Users.AsQueryable();   

        // Filtrera användare baserat på roll (om en roll är vald)
        if (!string.IsNullOrEmpty(roleFilter))
        {
            var roleUsers = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, roleFilter))
                {
                    roleUsers.Add(user);
                }
            }
            users = roleUsers.AsQueryable();
        }

        // Sök efter användare baserat på sökord
        if (!string.IsNullOrEmpty(searchString))
        {
            users = users.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));
        }

        // Skapa en lista med roller för dropdown i vyn
        var roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Name", "Name", roleFilter);

        // Hämta roller för alla användare och skapa en lista av UserRolesViewModel
        var userRoles = new List<UserRolesViewModel>();

        foreach (var user in users)
        {
            var rolesForUser = await _userManager.GetRolesAsync(user);
            userRoles.Add(new UserRolesViewModel
            {
                User = user,
                Roles = rolesForUser
            });
        }

        // Skicka både sökstring och roleFilter till vyn
        ViewData["SearchString"] = searchString;
        ViewData["RoleFilter"] = roleFilter;

        // Skicka UserRolesViewModel-listan till vyn
        return View(userRoles);
    }

    // Admin: Skapa en användare och tilldela en roll (Stylist eller annan roll)
    [HttpGet]
    public IActionResult Create()
    {
        var model = new AdminCreateUserViewModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdminCreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Firstname = model.Firstname, Lastname = model.Lastname };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Tilldela den valda rollen ("Stylist" eller "Admin")
                if (await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Den valda rollen existerar inte.");
                    return View(model);
                }
                return RedirectToAction("Index"); // Om du vill redirigera till en lista med användare eller annan sida
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }
}
