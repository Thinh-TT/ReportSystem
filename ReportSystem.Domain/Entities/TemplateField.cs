namespace ReportSystem.Domain.Entities;

public class TemplateField
{
    public long Id { get; set; }

    public long TemplateVersionId { get; set; }

    public string FieldCode { get; set; } = string.Empty;

    public string FieldLabel { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public string? Unit { get; set; }

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public string? Placeholder { get; set; }

    public string? OptionsJson { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ReportTemplateVersion TemplateVersion { get; set; } = null!;

    public ICollection<FieldRule> Rules { get; set; } = new List<FieldRule>();

    public ICollection<ReportFieldValue> FieldValues { get; set; } = new List<ReportFieldValue>();
}
