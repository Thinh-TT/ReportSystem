using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Web.Security;

namespace ReportSystem.Web.Controllers;

[Authorize(Roles = RoleGroups.ManagerOrAdmin)]
public sealed class ManagerController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
