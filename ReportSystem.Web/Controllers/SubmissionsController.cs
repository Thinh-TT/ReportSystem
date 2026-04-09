using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Web.Security;

namespace ReportSystem.Web.Controllers;

[Authorize(Roles = RoleGroups.ManagerOrAdmin)]
[Route("submissions")]
public sealed class SubmissionsController : Controller
{
    [HttpGet("{id:long}")]
    public IActionResult Detail([FromRoute] long id)
    {
        ViewData["SubmissionId"] = id;
        return View();
    }
}
