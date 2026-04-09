using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Application.Services.Templates;
using ReportSystem.Application.Services.Workflow;
using ReportSystem.Domain.Entities;
using ReportSystem.Infrastructure.Data;
using ReportSystem.Web.Security;

namespace ReportSystem.Web.Controllers.Management;

[ApiController]
[Route("api/management")]
[Authorize]
public sealed class TemplateManagementController : ControllerBase
{
    private readonly ReportSystemDbContext _dbContext;
    private readonly ITemplateWorkflowService _templateWorkflowService;

    public TemplateManagementController(
        ReportSystemDbContext dbContext,
        ITemplateWorkflowService templateWorkflowService)
    {
        _dbContext = dbContext;
        _templateWorkflowService = templateWorkflowService;
    }

    [HttpGet("report-templates")]
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
    {
        var templates = await _dbContext.ReportTemplates
            .AsNoTracking()
            .OrderBy(x => x.TemplateCode)
            .ToListAsync(cancellationToken);

        return Ok(templates.Select(MapTemplate));
    }

    [HttpGet("report-templates/{id:long}")]
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> GetTemplate([FromRoute] long id, CancellationToken cancellationToken)
    {
        var template = await _dbContext.ReportTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return template is null ? NotFound() : Ok(MapTemplate(template));
    }

    [HttpPost("report-templates")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateTemplate([FromBody] ReportTemplateUpsertRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _templateWorkflowService.CreateTemplateAsync(
                new CreateTemplateWorkflowRequest
                {
                    TemplateCode = request.TemplateCode,
                    TemplateName = request.TemplateName,
                    Description = request.Description,
                    IsActive = request.IsActive
                },
                cancellationToken);

            return CreatedAtAction(nameof(GetTemplate), new { id = created.Id }, MapTemplate(created));
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("report-templates/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateTemplate(
        [FromRoute] long id,
        [FromBody] ReportTemplateUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _templateWorkflowService.UpdateTemplateAsync(
                id,
                new UpdateTemplateWorkflowRequest
                {
                    TemplateCode = request.TemplateCode,
                    TemplateName = request.TemplateName,
                    Description = request.Description,
                    IsActive = request.IsActive
                },
                cancellationToken);

            return Ok(MapTemplate(updated));
        }
        catch (WorkflowNotFoundException)
        {
            return NotFound();
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("report-templates/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
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
    [Authorize(Roles = RoleGroups.AllRoles)]
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
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> GetTemplateVersion([FromRoute] long id, CancellationToken cancellationToken)
    {
        var version = await _dbContext.ReportTemplateVersions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return version is null ? NotFound() : Ok(MapTemplateVersion(version));
    }

    [HttpPost("report-template-versions")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateTemplateVersion(
        [FromBody] ReportTemplateVersionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _templateWorkflowService.CreateVersionAsync(
                new CreateTemplateVersionWorkflowRequest
                {
                    TemplateId = request.TemplateId,
                    VersionNo = request.VersionNo,
                    Status = request.Status,
                    EffectiveFrom = request.EffectiveFrom,
                    EffectiveTo = request.EffectiveTo,
                    PublishedBy = request.PublishedBy,
                    PublishedAt = request.PublishedAt
                },
                cancellationToken);

            return CreatedAtAction(nameof(GetTemplateVersion), new { id = created.Id }, MapTemplateVersion(created));
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("report-template-versions/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateTemplateVersion(
        [FromRoute] long id,
        [FromBody] ReportTemplateVersionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _templateWorkflowService.UpdateVersionAsync(
                id,
                new UpdateTemplateVersionWorkflowRequest
                {
                    TemplateId = request.TemplateId,
                    VersionNo = request.VersionNo,
                    Status = request.Status,
                    EffectiveFrom = request.EffectiveFrom,
                    EffectiveTo = request.EffectiveTo,
                    PublishedBy = request.PublishedBy,
                    PublishedAt = request.PublishedAt
                },
                cancellationToken);

            return Ok(MapTemplateVersion(updated));
        }
        catch (WorkflowNotFoundException)
        {
            return NotFound();
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("report-template-versions/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
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
    [Authorize(Roles = RoleGroups.AllRoles)]
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
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> GetTemplateField([FromRoute] long id, CancellationToken cancellationToken)
    {
        var field = await _dbContext.TemplateFields
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return field is null ? NotFound() : Ok(MapTemplateField(field));
    }

    [HttpPost("template-fields")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateTemplateField(
        [FromBody] TemplateFieldUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _templateWorkflowService.CreateFieldAsync(
                new CreateTemplateFieldWorkflowRequest
                {
                    TemplateVersionId = request.TemplateVersionId,
                    FieldCode = request.FieldCode,
                    FieldLabel = request.FieldLabel,
                    DataType = request.DataType,
                    Unit = request.Unit,
                    IsRequired = request.IsRequired,
                    DisplayOrder = request.DisplayOrder,
                    Placeholder = request.Placeholder,
                    OptionsJson = request.OptionsJson,
                    IsActive = request.IsActive
                },
                cancellationToken);

            return CreatedAtAction(nameof(GetTemplateField), new { id = created.Id }, MapTemplateField(created));
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("template-fields/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateTemplateField(
        [FromRoute] long id,
        [FromBody] TemplateFieldUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _templateWorkflowService.UpdateFieldAsync(
                id,
                new UpdateTemplateFieldWorkflowRequest
                {
                    TemplateVersionId = request.TemplateVersionId,
                    FieldCode = request.FieldCode,
                    FieldLabel = request.FieldLabel,
                    DataType = request.DataType,
                    Unit = request.Unit,
                    IsRequired = request.IsRequired,
                    DisplayOrder = request.DisplayOrder,
                    Placeholder = request.Placeholder,
                    OptionsJson = request.OptionsJson,
                    IsActive = request.IsActive
                },
                cancellationToken);

            return Ok(MapTemplateField(updated));
        }
        catch (WorkflowNotFoundException)
        {
            return NotFound();
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("template-fields/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
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
    [Authorize(Roles = RoleGroups.AllRoles)]
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
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> GetFieldRule([FromRoute] long id, CancellationToken cancellationToken)
    {
        var rule = await _dbContext.FieldRules
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return rule is null ? NotFound() : Ok(MapFieldRule(rule));
    }

    [HttpPost("field-rules")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateFieldRule(
        [FromBody] FieldRuleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _templateWorkflowService.CreateRuleAsync(
                new CreateFieldRuleWorkflowRequest
                {
                    FieldId = request.FieldId,
                    RuleOrder = request.RuleOrder,
                    RuleType = request.RuleType,
                    MinValue = request.MinValue,
                    MaxValue = request.MaxValue,
                    ThresholdValue = request.ThresholdValue,
                    ExpectedText = request.ExpectedText,
                    Severity = request.Severity,
                    FailMessage = request.FailMessage,
                    IsActive = request.IsActive
                },
                cancellationToken);

            return CreatedAtAction(nameof(GetFieldRule), new { id = created.Id }, MapFieldRule(created));
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("field-rules/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateFieldRule(
        [FromRoute] long id,
        [FromBody] FieldRuleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _templateWorkflowService.UpdateRuleAsync(
                id,
                new UpdateFieldRuleWorkflowRequest
                {
                    FieldId = request.FieldId,
                    RuleOrder = request.RuleOrder,
                    RuleType = request.RuleType,
                    MinValue = request.MinValue,
                    MaxValue = request.MaxValue,
                    ThresholdValue = request.ThresholdValue,
                    ExpectedText = request.ExpectedText,
                    Severity = request.Severity,
                    FailMessage = request.FailMessage,
                    IsActive = request.IsActive
                },
                cancellationToken);

            return Ok(MapFieldRule(updated));
        }
        catch (WorkflowNotFoundException)
        {
            return NotFound();
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("field-rules/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
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

    private static ReportTemplateResponse MapTemplate(TemplateWorkflowTemplateResult template)
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

    private static ReportTemplateVersionResponse MapTemplateVersion(TemplateWorkflowVersionResult version)
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

    private static TemplateFieldResponse MapTemplateField(TemplateWorkflowFieldResult field)
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

    private static FieldRuleResponse MapFieldRule(TemplateWorkflowRuleResult rule)
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
