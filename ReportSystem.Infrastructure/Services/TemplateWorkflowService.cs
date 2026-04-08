using Microsoft.EntityFrameworkCore;
using ReportSystem.Application.Services.Templates;
using ReportSystem.Application.Services.Workflow;
using ReportSystem.Domain.Entities;
using ReportSystem.Infrastructure.Data;

namespace ReportSystem.Infrastructure.Services;

public sealed class TemplateWorkflowService : ITemplateWorkflowService
{
    private readonly ReportSystemDbContext _dbContext;

    public TemplateWorkflowService(ReportSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TemplateWorkflowTemplateResult> CreateTemplateAsync(
        CreateTemplateWorkflowRequest request,
        CancellationToken cancellationToken = default)
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
        await SaveChangesAsync(cancellationToken);
        return MapTemplate(template);
    }

    public async Task<TemplateWorkflowTemplateResult> UpdateTemplateAsync(
        long templateId,
        UpdateTemplateWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await _dbContext.ReportTemplates
            .SingleOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (template is null)
        {
            throw new WorkflowNotFoundException($"Template `{templateId}` was not found.");
        }

        template.TemplateCode = request.TemplateCode.Trim();
        template.TemplateName = request.TemplateName.Trim();
        template.Description = request.Description?.Trim();
        template.IsActive = request.IsActive;
        template.UpdatedAt = DateTime.UtcNow;

        await SaveChangesAsync(cancellationToken);
        return MapTemplate(template);
    }

    public async Task<TemplateWorkflowVersionResult> CreateVersionAsync(
        CreateTemplateVersionWorkflowRequest request,
        CancellationToken cancellationToken = default)
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
        await SaveChangesAsync(cancellationToken);
        return MapVersion(version);
    }

    public async Task<TemplateWorkflowVersionResult> UpdateVersionAsync(
        long templateVersionId,
        UpdateTemplateVersionWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.ReportTemplateVersions
            .SingleOrDefaultAsync(x => x.Id == templateVersionId, cancellationToken);
        if (version is null)
        {
            throw new WorkflowNotFoundException($"Template version `{templateVersionId}` was not found.");
        }

        version.TemplateId = request.TemplateId;
        version.VersionNo = request.VersionNo;
        version.Status = request.Status.Trim();
        version.EffectiveFrom = ToUtc(request.EffectiveFrom);
        version.EffectiveTo = ToUtc(request.EffectiveTo);
        version.PublishedBy = request.PublishedBy;
        version.PublishedAt = ToUtc(request.PublishedAt);
        version.UpdatedAt = DateTime.UtcNow;

        await SaveChangesAsync(cancellationToken);
        return MapVersion(version);
    }

    public async Task<TemplateWorkflowFieldResult> CreateFieldAsync(
        CreateTemplateFieldWorkflowRequest request,
        CancellationToken cancellationToken = default)
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
        await SaveChangesAsync(cancellationToken);
        return MapField(field);
    }

    public async Task<TemplateWorkflowFieldResult> UpdateFieldAsync(
        long fieldId,
        UpdateTemplateFieldWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        var field = await _dbContext.TemplateFields
            .SingleOrDefaultAsync(x => x.Id == fieldId, cancellationToken);
        if (field is null)
        {
            throw new WorkflowNotFoundException($"Template field `{fieldId}` was not found.");
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

        await SaveChangesAsync(cancellationToken);
        return MapField(field);
    }

    public async Task<TemplateWorkflowRuleResult> CreateRuleAsync(
        CreateFieldRuleWorkflowRequest request,
        CancellationToken cancellationToken = default)
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
        await SaveChangesAsync(cancellationToken);
        return MapRule(rule);
    }

    public async Task<TemplateWorkflowRuleResult> UpdateRuleAsync(
        long ruleId,
        UpdateFieldRuleWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.FieldRules
            .SingleOrDefaultAsync(x => x.Id == ruleId, cancellationToken);
        if (rule is null)
        {
            throw new WorkflowNotFoundException($"Field rule `{ruleId}` was not found.");
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

        await SaveChangesAsync(cancellationToken);
        return MapRule(rule);
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new WorkflowRuleViolationException(ex.InnerException?.Message ?? ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new WorkflowRuleViolationException(ex.Message);
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

    private static TemplateWorkflowTemplateResult MapTemplate(ReportTemplate template)
    {
        return new TemplateWorkflowTemplateResult
        {
            Id = template.Id,
            TemplateCode = template.TemplateCode,
            TemplateName = template.TemplateName,
            Description = template.Description,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    private static TemplateWorkflowVersionResult MapVersion(ReportTemplateVersion version)
    {
        return new TemplateWorkflowVersionResult
        {
            Id = version.Id,
            TemplateId = version.TemplateId,
            VersionNo = version.VersionNo,
            Status = version.Status,
            EffectiveFrom = version.EffectiveFrom,
            EffectiveTo = version.EffectiveTo,
            PublishedBy = version.PublishedBy,
            PublishedAt = version.PublishedAt,
            CreatedAt = version.CreatedAt,
            UpdatedAt = version.UpdatedAt
        };
    }

    private static TemplateWorkflowFieldResult MapField(TemplateField field)
    {
        return new TemplateWorkflowFieldResult
        {
            Id = field.Id,
            TemplateVersionId = field.TemplateVersionId,
            FieldCode = field.FieldCode,
            FieldLabel = field.FieldLabel,
            DataType = field.DataType,
            Unit = field.Unit,
            IsRequired = field.IsRequired,
            DisplayOrder = field.DisplayOrder,
            Placeholder = field.Placeholder,
            OptionsJson = field.OptionsJson,
            IsActive = field.IsActive,
            CreatedAt = field.CreatedAt,
            UpdatedAt = field.UpdatedAt
        };
    }

    private static TemplateWorkflowRuleResult MapRule(FieldRule rule)
    {
        return new TemplateWorkflowRuleResult
        {
            Id = rule.Id,
            FieldId = rule.FieldId,
            RuleOrder = rule.RuleOrder,
            RuleType = rule.RuleType,
            MinValue = rule.MinValue,
            MaxValue = rule.MaxValue,
            ThresholdValue = rule.ThresholdValue,
            ExpectedText = rule.ExpectedText,
            Severity = rule.Severity,
            FailMessage = rule.FailMessage,
            IsActive = rule.IsActive,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }
}
