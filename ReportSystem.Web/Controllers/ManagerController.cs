using Microsoft.AspNetCore.Mvc;

namespace ReportSystem.Web.Controllers;

public sealed class ManagerController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
