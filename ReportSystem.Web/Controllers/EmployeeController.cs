using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Web.Security;

namespace ReportSystem.Web.Controllers;

[Authorize(Roles = RoleGroups.EmployeeOrAdmin)]
public sealed class EmployeeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
