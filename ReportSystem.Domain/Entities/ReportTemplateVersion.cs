namespace ReportSystem.Domain.Entities;

public class ReportTemplateVersion
{
    public long Id { get; set; }

    public long TemplateId { get; set; }

    public int VersionNo { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public Guid? PublishedBy { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ReportTemplate Template { get; set; } = null!;

    public User? PublishedByUser { get; set; }

    public ICollection<TemplateField> Fields { get; set; } = new List<TemplateField>();

    public ICollection<ReportSubmission> Submissions { get; set; } = new List<ReportSubmission>();
}
