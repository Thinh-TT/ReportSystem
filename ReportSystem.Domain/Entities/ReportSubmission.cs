namespace ReportSystem.Domain.Entities;

public class ReportSubmission
{
    public long Id { get; set; }

    public string SubmissionNo { get; set; } = string.Empty;

    public long TemplateVersionId { get; set; }

    public DateOnly ReportDate { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string? PerformedByText { get; set; }

    public string Status { get; set; } = string.Empty;

    public string AutoResult { get; set; } = "PENDING";

    public string ManagerResult { get; set; } = "PENDING";

    public string? ManagerNote { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? EvaluatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ReportTemplateVersion TemplateVersion { get; set; } = null!;

    public User CreatedByUser { get; set; } = null!;

    public User? ApprovedByUser { get; set; }

    public ICollection<ReportFieldValue> FieldValues { get; set; } = new List<ReportFieldValue>();

    public ICollection<ReportAttachment> Attachments { get; set; } = new List<ReportAttachment>();

    public ICollection<ApprovalLog> ApprovalLogs { get; set; } = new List<ApprovalLog>();
}
