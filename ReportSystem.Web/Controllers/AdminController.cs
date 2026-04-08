using Microsoft.AspNetCore.Mvc;

namespace ReportSystem.Web.Controllers;

public sealed class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
