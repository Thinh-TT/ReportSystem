using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Infrastructure.Data;
using ReportSystem.Web.Models;
using ReportSystem.Web.Security;

namespace ReportSystem.Web.Controllers;

[AllowAnonymous]
[Route("account")]
public sealed class AccountController : Controller
{
    private readonly ReportSystemDbContext _dbContext;

    public AccountController(ReportSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocalOrDefault(returnUrl, User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value));
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] LoginViewModel model,
        [FromQuery] string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var employeeCode = model.EmployeeCode.Trim();
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.IsActive && x.EmployeeCode == employeeCode)
            .Select(x => new
            {
                x.Id,
                x.EmployeeCode,
                x.FullName,
                Roles = x.UserRoles.Select(ur => ur.Role.Code).ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid employee code or inactive user.");
            return View(model);
        }

        if (user.Roles.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "User does not have any assigned role.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new("employee_code", user.EmployeeCode)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);

        return RedirectToLocalOrDefault(returnUrl, user.Roles);
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("denied")]
    public IActionResult Denied()
    {
        return View();
    }

    private IActionResult RedirectToLocalOrDefault(string? returnUrl, IEnumerable<string> roles)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roleSet.Contains(RoleNames.Admin))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (roleSet.Contains(RoleNames.Manager))
        {
            return RedirectToAction("Index", "Manager");
        }

        if (roleSet.Contains(RoleNames.Employee))
        {
            return RedirectToAction("Index", "Employee");
        }

        return RedirectToAction("Index", "Home");
    }
}
