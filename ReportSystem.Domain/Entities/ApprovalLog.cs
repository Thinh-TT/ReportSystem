namespace ReportSystem.Domain.Entities;

public class ApprovalLog
{
    public long Id { get; set; }

    public long SubmissionId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }

    public Guid? ActionByUserId { get; set; }

    public string? Comment { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime ActionAt { get; set; }

    public ReportSubmission Submission { get; set; } = null!;

    public User? ActionByUser { get; set; }
}
