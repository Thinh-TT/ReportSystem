namespace ReportSystem.Application.Services.Workflow;

public sealed class SubmissionWorkflowResult
{
    public long SubmissionId { get; init; }
    public string SubmissionNo { get; init; } = string.Empty;
    public long TemplateVersionId { get; init; }
    public DateOnly ReportDate { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string? PerformedByText { get; init; }
    public string Status { get; init; } = string.Empty;
    public string AutoResult { get; init; } = string.Empty;
    public string ManagerResult { get; init; } = string.Empty;
    public string? ManagerNote { get; init; }
    public Guid? ApprovedByUserId { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? EvaluatedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyCollection<SubmissionWorkflowLogItem> Logs { get; init; } = Array.Empty<SubmissionWorkflowLogItem>();
}

public sealed class SubmissionWorkflowLogItem
{
    public long LogId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string? FromStatus { get; init; }
    public string? ToStatus { get; init; }
    public Guid? ActionByUserId { get; init; }
    public string? Comment { get; init; }
    public string? MetadataJson { get; init; }
    public DateTime ActionAt { get; init; }
}

public sealed class SubmissionListQueryRequest
{
    public long? TemplateVersionId { get; init; }
    public DateOnly? ReportDateFrom { get; init; }
    public DateOnly? ReportDateTo { get; init; }
    public string? Status { get; init; }
    public Guid? CreatedByUserId { get; init; }
}

public sealed class SubmissionListItem
{
    public long SubmissionId { get; init; }
    public string SubmissionNo { get; init; } = string.Empty;
    public long TemplateVersionId { get; init; }
    public DateOnly ReportDate { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string AutoResult { get; init; } = string.Empty;
    public string ManagerResult { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class CreateDraftSubmissionRequest
{
    public long TemplateVersionId { get; init; }
    public DateOnly ReportDate { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string? PerformedByText { get; init; }
}

public sealed class SubmissionFieldValueInput
{
    public long FieldId { get; init; }
    public string? ValueText { get; init; }
    public decimal? ValueNumber { get; init; }
    public DateOnly? ValueDate { get; init; }
    public DateTime? ValueDateTime { get; init; }
    public bool? ValueBool { get; init; }
}

public sealed class UpdateSubmissionFieldValuesRequest
{
    public long SubmissionId { get; init; }
    public IReadOnlyCollection<SubmissionFieldValueInput> FieldValues { get; init; } = Array.Empty<SubmissionFieldValueInput>();
}

public sealed class SubmitSubmissionRequest
{
    public long SubmissionId { get; init; }
    public Guid ActionByUserId { get; init; }
}

public sealed class AutoEvaluateSubmissionRequest
{
    public long SubmissionId { get; init; }
}

public sealed class ApproveSubmissionRequest
{
    public long SubmissionId { get; init; }
    public Guid ActionByUserId { get; init; }
    public string ManagerResult { get; init; } = "PASS";
    public string? ManagerNote { get; init; }
}

public sealed class RejectSubmissionRequest
{
    public long SubmissionId { get; init; }
    public Guid ActionByUserId { get; init; }
    public string? ManagerNote { get; init; }
}

public sealed class ReopenSubmissionRequest
{
    public long SubmissionId { get; init; }
    public Guid ActionByUserId { get; init; }
    public string? Reason { get; init; }
}
