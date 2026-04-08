using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Domain.Entities;
using ReportSystem.Infrastructure.Data;

namespace ReportSystem.Web.Controllers.Management;

[ApiController]
[Route("api/management")]
public sealed class TemplateManagementController : ControllerBase
{
    private readonly ReportSystemDbContext _dbContext;

    public TemplateManagementController(ReportSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("report-templates")]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
    {
        var templates = await _dbContext.ReportTemplates
            .AsNoTracking()
            .OrderBy(x => x.TemplateCode)
            .ToListAsync(cancellationToken);

        return Ok(templates.Select(MapTemplate));
    }

    [HttpGet("report-templates/{id:long}")]
    public async Task<IActionResult> GetTemplate([FromRoute] long id, CancellationToken cancellationToken)
    {
        var template = await _dbContext.ReportTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return template is null ? NotFound() : Ok(MapTemplate(template));
    }

    [HttpPost("report-templates")]
    public async Task<IActionResult> CreateTemplate([FromBody] ReportTemplateUpsertRequest request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var template = new ReportTemplate
        {
            TemplateCode = request.TemplateCode.Trim(),
            TemplateName = request.TemplateName.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.ReportTemplates.Add(template);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, MapTemplate(template));
    }

    [HttpPut("report-templates/{id:long}")]
    public async Task<IActionResult> UpdateTemplate(
        [FromRoute] long id,
        [FromBody] ReportTemplateUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.ReportTemplates.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (template is null)
        {
            return NotFound();
        }

        template.TemplateCode = request.TemplateCode.Trim();
        template.TemplateName = request.TemplateName.Trim();
        template.Description = request.Description?.Trim();
        template.IsActive = request.IsActive;
        template.UpdatedAt = DateTime.UtcNow;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapTemplate(template));
    }

    [HttpDelete("report-templates/{id:long}")]
    public async Task<IActionResult> DeleteTemplate([FromRoute] long id, CancellationToken cancellationToken)
    {
        var template = await _dbContext.ReportTemplates.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (template is null)
        {
            return NotFound();
        }

        _dbContext.ReportTemplates.Remove(template);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    [HttpGet("report-template-versions")]
    public async Task<IActionResult> GetTemplateVersions(CancellationToken cancellationToken)
    {
        var versions = await _dbContext.ReportTemplateVersions
            .AsNoTracking()
            .OrderBy(x => x.TemplateId)
            .ThenBy(x => x.VersionNo)
            .ToListAsync(cancellationToken);

        return Ok(versions.Select(MapTemplateVersion));
    }

    [HttpGet("report-template-versions/{id:long}")]
    public async Task<IActionResult> GetTemplateVersion([FromRoute] long id, CancellationToken cancellationToken)
    {
        var version = await _dbContext.ReportTemplateVersions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return version is null ? NotFound() : Ok(MapTemplateVersion(version));
    }

    [HttpPost("report-template-versions")]
    public async Task<IActionResult> CreateTemplateVersion(
        [FromBody] ReportTemplateVersionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var version = new ReportTemplateVersion
        {
            TemplateId = request.TemplateId,
            VersionNo = request.VersionNo,
            Status = request.Status.Trim(),
            EffectiveFrom = ToUtc(request.EffectiveFrom),
            EffectiveTo = ToUtc(request.EffectiveTo),
            PublishedBy = request.PublishedBy,
            PublishedAt = ToUtc(request.PublishedAt),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.ReportTemplateVersions.Add(version);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetTemplateVersion), new { id = version.Id }, MapTemplateVersion(version));
    }

    [HttpPut("report-template-versions/{id:long}")]
    public async Task<IActionResult> UpdateTemplateVersion(
        [FromRoute] long id,
        [FromBody] ReportTemplateVersionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var version = await _dbContext.ReportTemplateVersions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (version is null)
        {
            return NotFound();
        }

        version.TemplateId = request.TemplateId;
        version.VersionNo = request.VersionNo;
        version.Status = request.Status.Trim();
        version.EffectiveFrom = ToUtc(request.EffectiveFrom);
        version.EffectiveTo = ToUtc(request.EffectiveTo);
        version.PublishedBy = request.PublishedBy;
        version.PublishedAt = ToUtc(request.PublishedAt);
        version.UpdatedAt = DateTime.UtcNow;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapTemplateVersion(version));
    }

    [HttpDelete("report-template-versions/{id:long}")]
    public async Task<IActionResult> DeleteTemplateVersion([FromRoute] long id, CancellationToken cancellationToken)
    {
        var version = await _dbContext.ReportTemplateVersions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (version is null)
        {
            return NotFound();
        }

        _dbContext.ReportTemplateVersions.Remove(version);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    [HttpGet("template-fields")]
    public async Task<IActionResult> GetTemplateFields(CancellationToken cancellationToken)
    {
        var fields = await _dbContext.TemplateFields
            .AsNoTracking()
            .OrderBy(x => x.TemplateVersionId)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        return Ok(fields.Select(MapTemplateField));
    }

    [HttpGet("template-fields/{id:long}")]
    public async Task<IActionResult> GetTemplateField([FromRoute] long id, CancellationToken cancellationToken)
    {
        var field = await _dbContext.TemplateFields
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return field is null ? NotFound() : Ok(MapTemplateField(field));
    }

    [HttpPost("template-fields")]
    public async Task<IActionResult> CreateTemplateField(
        [FromBody] TemplateFieldUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var field = new TemplateField
        {
            TemplateVersionId = request.TemplateVersionId,
            FieldCode = request.FieldCode.Trim(),
            FieldLabel = request.FieldLabel.Trim(),
            DataType = request.DataType.Trim(),
            Unit = request.Unit?.Trim(),
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder,
            Placeholder = request.Placeholder?.Trim(),
            OptionsJson = request.OptionsJson,
            IsActive = request.IsActive,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.TemplateFields.Add(field);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetTemplateField), new { id = field.Id }, MapTemplateField(field));
    }

    [HttpPut("template-fields/{id:long}")]
    public async Task<IActionResult> UpdateTemplateField(
        [FromRoute] long id,
        [FromBody] TemplateFieldUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var field = await _dbContext.TemplateFields.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (field is null)
        {
            return NotFound();
        }

        field.TemplateVersionId = request.TemplateVersionId;
        field.FieldCode = request.FieldCode.Trim();
        field.FieldLabel = request.FieldLabel.Trim();
        field.DataType = request.DataType.Trim();
        field.Unit = request.Unit?.Trim();
        field.IsRequired = request.IsRequired;
        field.DisplayOrder = request.DisplayOrder;
        field.Placeholder = request.Placeholder?.Trim();
        field.OptionsJson = request.OptionsJson;
        field.IsActive = request.IsActive;
        field.UpdatedAt = DateTime.UtcNow;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapTemplateField(field));
    }

    [HttpDelete("template-fields/{id:long}")]
    public async Task<IActionResult> DeleteTemplateField([FromRoute] long id, CancellationToken cancellationToken)
    {
        var field = await _dbContext.TemplateFields.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (field is null)
        {
            return NotFound();
        }

        _dbContext.TemplateFields.Remove(field);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    [HttpGet("field-rules")]
    public async Task<IActionResult> GetFieldRules(CancellationToken cancellationToken)
    {
        var rules = await _dbContext.FieldRules
            .AsNoTracking()
            .OrderBy(x => x.FieldId)
            .ThenBy(x => x.RuleOrder)
            .ToListAsync(cancellationToken);

        return Ok(rules.Select(MapFieldRule));
    }

    [HttpGet("field-rules/{id:long}")]
    public async Task<IActionResult> GetFieldRule([FromRoute] long id, CancellationToken cancellationToken)
    {
        var rule = await _dbContext.FieldRules
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return rule is null ? NotFound() : Ok(MapFieldRule(rule));
    }

    [HttpPost("field-rules")]
    public async Task<IActionResult> CreateFieldRule(
        [FromBody] FieldRuleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var rule = new FieldRule
        {
            FieldId = request.FieldId,
            RuleOrder = request.RuleOrder,
            RuleType = request.RuleType.Trim(),
            MinValue = request.MinValue,
            MaxValue = request.MaxValue,
            ThresholdValue = request.ThresholdValue,
            ExpectedText = request.ExpectedText?.Trim(),
            Severity = request.Severity.Trim(),
            FailMessage = request.FailMessage?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.FieldRules.Add(rule);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetFieldRule), new { id = rule.Id }, MapFieldRule(rule));
    }

    [HttpPut("field-rules/{id:long}")]
    public async Task<IActionResult> UpdateFieldRule(
        [FromRoute] long id,
        [FromBody] FieldRuleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await _dbContext.FieldRules.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (rule is null)
        {
            return NotFound();
        }

        rule.FieldId = request.FieldId;
        rule.RuleOrder = request.RuleOrder;
        rule.RuleType = request.RuleType.Trim();
        rule.MinValue = request.MinValue;
        rule.MaxValue = request.MaxValue;
        rule.ThresholdValue = request.ThresholdValue;
        rule.ExpectedText = request.ExpectedText?.Trim();
        rule.Severity = request.Severity.Trim();
        rule.FailMessage = request.FailMessage?.Trim();
        rule.IsActive = request.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapFieldRule(rule));
    }

    [HttpDelete("field-rules/{id:long}")]
    public async Task<IActionResult> DeleteFieldRule([FromRoute] long id, CancellationToken cancellationToken)
    {
        var rule = await _dbContext.FieldRules.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (rule is null)
        {
            return NotFound();
        }

        _dbContext.FieldRules.Remove(rule);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    private async Task<IActionResult?> TrySaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value.Kind == DateTimeKind.Utc
            ? value.Value
            : value.Value.ToUniversalTime();
    }

    private static ReportTemplateResponse MapTemplate(ReportTemplate template)
    {
        return new ReportTemplateResponse(
            template.Id,
            template.TemplateCode,
            template.TemplateName,
            template.Description,
            template.IsActive,
            template.CreatedAt,
            template.UpdatedAt);
    }

    private static ReportTemplateVersionResponse MapTemplateVersion(ReportTemplateVersion version)
    {
        return new ReportTemplateVersionResponse(
            version.Id,
            version.TemplateId,
            version.VersionNo,
            version.Status,
            version.EffectiveFrom,
            version.EffectiveTo,
            version.PublishedBy,
            version.PublishedAt,
            version.CreatedAt,
            version.UpdatedAt);
    }

    private static TemplateFieldResponse MapTemplateField(TemplateField field)
    {
        return new TemplateFieldResponse(
            field.Id,
            field.TemplateVersionId,
            field.FieldCode,
            field.FieldLabel,
            field.DataType,
            field.Unit,
            field.IsRequired,
            field.DisplayOrder,
            field.Placeholder,
            field.OptionsJson,
            field.IsActive,
            field.CreatedAt,
            field.UpdatedAt);
    }

    private static FieldRuleResponse MapFieldRule(FieldRule rule)
    {
        return new FieldRuleResponse(
            rule.Id,
            rule.FieldId,
            rule.RuleOrder,
            rule.RuleType,
            rule.MinValue,
            rule.MaxValue,
            rule.ThresholdValue,
            rule.ExpectedText,
            rule.Severity,
            rule.FailMessage,
            rule.IsActive,
            rule.CreatedAt,
            rule.UpdatedAt);
    }

    public sealed record ReportTemplateResponse(
        long Id,
        string TemplateCode,
        string TemplateName,
        string? Description,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed class ReportTemplateUpsertRequest
    {
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed record ReportTemplateVersionResponse(
        long Id,
        long TemplateId,
        int VersionNo,
        string Status,
        DateTime? EffectiveFrom,
        DateTime? EffectiveTo,
        Guid? PublishedBy,
        DateTime? PublishedAt,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed class ReportTemplateVersionUpsertRequest
    {
        public long TemplateId { get; set; }
        public int VersionNo { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public Guid? PublishedBy { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public sealed record TemplateFieldResponse(
        long Id,
        long TemplateVersionId,
        string FieldCode,
        string FieldLabel,
        string DataType,
        string? Unit,
        bool IsRequired,
        int DisplayOrder,
        string? Placeholder,
        string? OptionsJson,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed class TemplateFieldUpsertRequest
    {
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
    }

    public sealed record FieldRuleResponse(
        long Id,
        long FieldId,
        int RuleOrder,
        string RuleType,
        decimal? MinValue,
        decimal? MaxValue,
        decimal? ThresholdValue,
        string? ExpectedText,
        string Severity,
        string? FailMessage,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed class FieldRuleUpsertRequest
    {
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
    }
}
