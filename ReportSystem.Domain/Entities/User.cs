namespace ReportSystem.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<ReportTemplateVersion> PublishedTemplateVersions { get; set; } = new List<ReportTemplateVersion>();

    public ICollection<ReportSubmission> CreatedSubmissions { get; set; } = new List<ReportSubmission>();

    public ICollection<ReportSubmission> ApprovedSubmissions { get; set; } = new List<ReportSubmission>();

    public ICollection<ReportAttachment> UploadedAttachments { get; set; } = new List<ReportAttachment>();

    public ICollection<ApprovalLog> ApprovalLogs { get; set; } = new List<ApprovalLog>();
}
