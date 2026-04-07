namespace ReportSystem.Domain.Entities;

public class ReportAttachment
{
    public long Id { get; set; }

    public long SubmissionId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long? FileSizeBytes { get; set; }

    public DateTime? CapturedAt { get; set; }

    public Guid UploadedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public ReportSubmission Submission { get; set; } = null!;

    public User UploadedByUser { get; set; } = null!;
}
