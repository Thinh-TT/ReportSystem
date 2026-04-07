namespace ReportSystem.Domain.Entities;

public class ReportTemplate
{
    public long Id { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<ReportTemplateVersion> Versions { get; set; } = new List<ReportTemplateVersion>();
}
