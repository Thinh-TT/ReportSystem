namespace ReportSystem.Domain.Entities;

public class ReportFieldValue
{
    public long Id { get; set; }

    public long SubmissionId { get; set; }

    public long FieldId { get; set; }

    public string? ValueText { get; set; }

    public decimal? ValueNumber { get; set; }

    public DateOnly? ValueDate { get; set; }

    public DateTime? ValueDateTime { get; set; }

    public bool? ValueBool { get; set; }

    public string? NormalizedValue { get; set; }

    public string AutoResult { get; set; } = "PENDING";

    public string? EvaluationNote { get; set; }

    public string? RuleSnapshotJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ReportSubmission Submission { get; set; } = null!;

    public TemplateField Field { get; set; } = null!;
}
