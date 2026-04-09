using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Web.Security;

namespace ReportSystem.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
