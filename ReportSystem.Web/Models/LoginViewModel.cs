using System.ComponentModel.DataAnnotations;

namespace ReportSystem.Web.Models;

public sealed class LoginViewModel
{
    [Required]
    [Display(Name = "Employee Code")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
}
