namespace ReportSystem.Domain.Entities;

public class FieldRule
{
    public long Id { get; set; }

    public long FieldId { get; set; }

    public int RuleOrder { get; set; } = 1;

    public string RuleType { get; set; } = string.Empty;

    public decimal? MinValue { get; set; }

    public decimal? MaxValue { get; set; }

    public decimal? ThresholdValue { get; set; }

    public string? ExpectedText { get; set; }

    public string Severity { get; set; } = "ERROR";

    public string? FailMessage { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public TemplateField Field { get; set; } = null!;
}
