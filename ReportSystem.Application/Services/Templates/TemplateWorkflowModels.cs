namespace ReportSystem.Application.Services.Templates;

public sealed class CreateTemplateWorkflowRequest
{
    public string TemplateCode { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UpdateTemplateWorkflowRequest
{
    public string TemplateCode { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class TemplateWorkflowTemplateResult
{
    public long Id { get; init; }
    public string TemplateCode { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class CreateTemplateVersionWorkflowRequest
{
    public long TemplateId { get; init; }
    public int VersionNo { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public Guid? PublishedBy { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public sealed class UpdateTemplateVersionWorkflowRequest
{
    public long TemplateId { get; init; }
    public int VersionNo { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public Guid? PublishedBy { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public sealed class TemplateWorkflowVersionResult
{
    public long Id { get; init; }
    public long TemplateId { get; init; }
    public int VersionNo { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public Guid? PublishedBy { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class CreateTemplateFieldWorkflowRequest
{
    public long TemplateVersionId { get; init; }
    public string FieldCode { get; init; } = string.Empty;
    public string FieldLabel { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? Placeholder { get; init; }
    public string? OptionsJson { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UpdateTemplateFieldWorkflowRequest
{
    public long TemplateVersionId { get; init; }
    public string FieldCode { get; init; } = string.Empty;
    public string FieldLabel { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? Placeholder { get; init; }
    public string? OptionsJson { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class TemplateWorkflowFieldResult
{
    public long Id { get; init; }
    public long TemplateVersionId { get; init; }
    public string FieldCode { get; init; } = string.Empty;
    public string FieldLabel { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? Placeholder { get; init; }
    public string? OptionsJson { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class CreateFieldRuleWorkflowRequest
{
    public long FieldId { get; init; }
    public int RuleOrder { get; init; } = 1;
    public string RuleType { get; init; } = string.Empty;
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public decimal? ThresholdValue { get; init; }
    public string? ExpectedText { get; init; }
    public string Severity { get; init; } = "ERROR";
    public string? FailMessage { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UpdateFieldRuleWorkflowRequest
{
    public long FieldId { get; init; }
    public int RuleOrder { get; init; } = 1;
    public string RuleType { get; init; } = string.Empty;
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public decimal? ThresholdValue { get; init; }
    public string? ExpectedText { get; init; }
    public string Severity { get; init; } = "ERROR";
    public string? FailMessage { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class TemplateWorkflowRuleResult
{
    public long Id { get; init; }
    public long FieldId { get; init; }
    public int RuleOrder { get; init; }
    public string RuleType { get; init; } = string.Empty;
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public decimal? ThresholdValue { get; init; }
    public string? ExpectedText { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string? FailMessage { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
