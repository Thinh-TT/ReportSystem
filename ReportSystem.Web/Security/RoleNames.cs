namespace ReportSystem.Web.Security;

public static class RoleNames
{
    public const string Employee = "EMPLOYEE";
    public const string Manager = "MANAGER";
    public const string Admin = "ADMIN";
}

public static class RoleGroups
{
    public const string EmployeeOrAdmin = RoleNames.Employee + "," + RoleNames.Admin;
    public const string ManagerOrAdmin = RoleNames.Manager + "," + RoleNames.Admin;
    public const string AllRoles = RoleNames.Employee + "," + RoleNames.Manager + "," + RoleNames.Admin;
}
