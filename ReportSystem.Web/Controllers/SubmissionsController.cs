using Microsoft.AspNetCore.Mvc;

namespace ReportSystem.Web.Controllers;

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
