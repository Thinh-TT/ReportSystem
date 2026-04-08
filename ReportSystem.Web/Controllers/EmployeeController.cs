using Microsoft.AspNetCore.Mvc;

namespace ReportSystem.Web.Controllers;

public sealed class EmployeeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
