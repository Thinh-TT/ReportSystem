using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Application.Services.Workflow;
using ReportSystem.Domain.Constants;
using ReportSystem.Domain.Entities;
using ReportSystem.Infrastructure.Data;

namespace ReportSystem.Infrastructure.Services;

public sealed class SubmissionWorkflowService : ISubmissionWorkflowService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ReportSystemDbContext _dbContext;

    public SubmissionWorkflowService(ReportSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<SubmissionListItem>> ListAsync(
        SubmissionListQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ReportDateFrom.HasValue &&
            request.ReportDateTo.HasValue &&
            request.ReportDateFrom.Value > request.ReportDateTo.Value)
        {
            throw new WorkflowRuleViolationException("ReportDateFrom must be less than or equal to ReportDateTo.");
        }

        var query = _dbContext.ReportSubmissions.AsNoTracking().AsQueryable();

        if (request.TemplateVersionId.HasValue)
        {
            query = query.Where(x => x.TemplateVersionId == request.TemplateVersionId.Value);
        }

        if (request.ReportDateFrom.HasValue)
        {
            query = query.Where(x => x.ReportDate >= request.ReportDateFrom.Value);
        }

        if (request.ReportDateTo.HasValue)
        {
            query = query.Where(x => x.ReportDate <= request.ReportDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
        }

        if (request.CreatedByUserId.HasValue)
        {
            query = query.Where(x => x.CreatedByUserId == request.CreatedByUserId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SubmissionListItem
            {
                SubmissionId = x.Id,
                SubmissionNo = x.SubmissionNo,
                TemplateVersionId = x.TemplateVersionId,
                ReportDate = x.ReportDate,
                CreatedByUserId = x.CreatedByUserId,
                Status = x.Status,
                AutoResult = x.AutoResult,
                ManagerResult = x.ManagerResult,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SubmissionWorkflowResult> CreateDraftAsync(
        CreateDraftSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var templateVersion = await _dbContext.ReportTemplateVersions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.TemplateVersionId, cancellationToken);

        if (templateVersion is null)
        {
            throw new WorkflowNotFoundException($"Template version `{request.TemplateVersionId}` was not found.");
        }

        if (!string.Equals(templateVersion.Status, TemplateVersionStatuses.Published, StringComparison.Ordinal))
        {
            throw new WorkflowRuleViolationException("Only PUBLISHED template versions can be used to create draft submissions.");
        }

        var userExists = await _dbContext.Users.AnyAsync(x => x.Id == request.CreatedByUserId, cancellationToken);
        if (!userExists)
        {
            throw new WorkflowNotFoundException($"User `{request.CreatedByUserId}` was not found.");
        }

        var utcNow = DateTime.UtcNow;

        var submission = new ReportSubmission
        {
            SubmissionNo = await GenerateSubmissionNoAsync(cancellationToken),
            TemplateVersionId = request.TemplateVersionId,
            ReportDate = request.ReportDate,
            CreatedByUserId = request.CreatedByUserId,
            PerformedByText = request.PerformedByText,
            Status = SubmissionStatuses.Draft,
            AutoResult = SubmissionAutoResults.Pending,
            ManagerResult = SubmissionAutoResults.Pending,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.ReportSubmissions.Add(submission);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await BuildWorkflowResultAsync(submission, cancellationToken);
    }

    public async Task<SubmissionWorkflowResult> UpdateFieldValuesAsync(
        UpdateSubmissionFieldValuesRequest request,
        CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.ReportSubmissions
            .SingleOrDefaultAsync(x => x.Id == request.SubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new WorkflowNotFoundException($"Submission `{request.SubmissionId}` was not found.");
        }

        EnsureSubmissionStatus(submission.Status, SubmissionStatuses.Draft, "Field values can only be updated when submission is in DRAFT status.");

        if (request.FieldValues.Count == 0)
        {
            throw new WorkflowRuleViolationException("At least one field value is required.");
        }

        var submittedFieldIds = request.FieldValues.Select(x => x.FieldId).ToArray();
        var allowedFields = await _dbContext.TemplateFields
            .Where(x => x.TemplateVersionId == submission.TemplateVersionId && submittedFieldIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var invalidFieldIds = submittedFieldIds.Where(x => !allowedFields.ContainsKey(x)).Distinct().ToArray();
        if (invalidFieldIds.Length > 0)
        {
            throw new WorkflowRuleViolationException(
                $"Invalid field ids for submission template version: {string.Join(", ", invalidFieldIds)}.");
        }

        var existingFieldValues = await _dbContext.ReportFieldValues
            .Where(x => x.SubmissionId == submission.Id && submittedFieldIds.Contains(x.FieldId))
            .ToDictionaryAsync(x => x.FieldId, cancellationToken);

        var utcNow = DateTime.UtcNow;
        foreach (var input in request.FieldValues)
        {
            if (!existingFieldValues.TryGetValue(input.FieldId, out var value))
            {
                value = new ReportFieldValue
                {
                    SubmissionId = submission.Id,
                    FieldId = input.FieldId,
                    CreatedAt = utcNow
                };
                _dbContext.ReportFieldValues.Add(value);
            }

            value.ValueText = input.ValueText;
            value.ValueNumber = input.ValueNumber;
            value.ValueDate = input.ValueDate;
            value.ValueDateTime = input.ValueDateTime;
            value.ValueBool = input.ValueBool;
            value.NormalizedValue = NormalizeValue(input);
            value.UpdatedAt = utcNow;
        }

        submission.UpdatedAt = utcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await BuildWorkflowResultAsync(submission, cancellationToken);
    }

    public async Task<SubmissionWorkflowResult> SubmitAsync(
        SubmitSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.ReportSubmissions
            .SingleOrDefaultAsync(x => x.Id == request.SubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new WorkflowNotFoundException($"Submission `{request.SubmissionId}` was not found.");
        }

        EnsureSubmissionStatus(submission.Status, SubmissionStatuses.Draft, "Only DRAFT submissions can be submitted.");
        await EnsureActionUserExistsAsync(request.ActionByUserId, cancellationToken);

        var requiredFields = await _dbContext.TemplateFields
            .Where(x => x.TemplateVersionId == submission.TemplateVersionId && x.IsActive && x.IsRequired)
            .Select(x => new { x.Id, x.FieldCode, x.DataType })
            .ToListAsync(cancellationToken);

        var requiredFieldIds = requiredFields.Select(x => x.Id).ToArray();
        var currentValues = await _dbContext.ReportFieldValues
            .Where(x => x.SubmissionId == submission.Id && requiredFieldIds.Contains(x.FieldId))
            .ToDictionaryAsync(x => x.FieldId, cancellationToken);

        var missingFields = requiredFields
            .Where(field => !currentValues.TryGetValue(field.Id, out var value) || !HasEffectiveValue(field.DataType, value))
            .Select(field => field.FieldCode)
            .ToArray();

        if (missingFields.Length > 0)
        {
            throw new WorkflowRuleViolationException(
                $"Cannot submit because required fields are missing values: {string.Join(", ", missingFields)}.");
        }

        var utcNow = DateTime.UtcNow;
        var fromStatus = submission.Status;

        submission.Status = SubmissionStatuses.Submitted;
        submission.SubmittedAt = utcNow;
        submission.UpdatedAt = utcNow;

        _dbContext.ApprovalLogs.Add(new ApprovalLog
        {
            SubmissionId = submission.Id,
            Action = ApprovalActions.Submit,
            FromStatus = fromStatus,
            ToStatus = submission.Status,
            ActionByUserId = request.ActionByUserId,
            ActionAt = utcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildWorkflowResultAsync(submission, cancellationToken);
    }

    public async Task<SubmissionWorkflowResult> AutoEvaluateAsync(
        AutoEvaluateSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.ReportSubmissions
            .SingleOrDefaultAsync(x => x.Id == request.SubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new WorkflowNotFoundException($"Submission `{request.SubmissionId}` was not found.");
        }

        EnsureSubmissionStatus(
            submission.Status,
            SubmissionStatuses.Submitted,
            "Only SUBMITTED submissions can be auto evaluated.");

        var fields = await _dbContext.TemplateFields
            .Where(x => x.TemplateVersionId == submission.TemplateVersionId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        var fieldIds = fields.Select(x => x.Id).ToArray();
        var fieldValues = await _dbContext.ReportFieldValues
            .Where(x => x.SubmissionId == submission.Id && fieldIds.Contains(x.FieldId))
            .ToDictionaryAsync(x => x.FieldId, cancellationToken);

        var rules = await _dbContext.FieldRules
            .Where(x => fieldIds.Contains(x.FieldId) && x.IsActive)
            .OrderBy(x => x.FieldId)
            .ThenBy(x => x.RuleOrder)
            .ToListAsync(cancellationToken);

        var rulesByField = rules.GroupBy(x => x.FieldId).ToDictionary(x => x.Key, x => x.ToList());
        var utcNow = DateTime.UtcNow;
        var submissionHasErrorFail = false;

        foreach (var field in fields)
        {
            fieldValues.TryGetValue(field.Id, out var fieldValue);

            if (fieldValue is null)
            {
                fieldValue = new ReportFieldValue
                {
                    SubmissionId = submission.Id,
                    FieldId = field.Id,
                    CreatedAt = utcNow
                };

                _dbContext.ReportFieldValues.Add(fieldValue);
                fieldValues[field.Id] = fieldValue;
            }

            var fieldRuleList = rulesByField.TryGetValue(field.Id, out var currentRules)
                ? currentRules
                : new List<FieldRule>();

            if (field.IsRequired && !HasEffectiveValue(field.DataType, fieldValue))
            {
                submissionHasErrorFail = true;
                fieldValue.AutoResult = SubmissionAutoResults.Fail;
                fieldValue.EvaluationNote = $"Required field `{field.FieldCode}` is missing a value.";
                fieldValue.RuleSnapshotJson = "[]";
                fieldValue.UpdatedAt = utcNow;
                continue;
            }

            if (fieldRuleList.Count == 0)
            {
                fieldValue.AutoResult = "NA";
                fieldValue.EvaluationNote = null;
                fieldValue.RuleSnapshotJson = "[]";
                fieldValue.UpdatedAt = utcNow;
                continue;
            }

            var evaluations = fieldRuleList
                .Select(rule => EvaluateRule(field, fieldValue, rule))
                .ToList();

            submissionHasErrorFail = submissionHasErrorFail || evaluations.Any(x => !x.IsPass && x.IsErrorSeverity);
            fieldValue.AutoResult = evaluations.Any(x => !x.IsPass) ? SubmissionAutoResults.Fail : SubmissionAutoResults.Pass;
            fieldValue.EvaluationNote = evaluations.FirstOrDefault(x => !x.IsPass)?.Message;
            fieldValue.RuleSnapshotJson = JsonSerializer.Serialize(
                evaluations.Select(x => new RuleSnapshotItem
                {
                    RuleOrder = x.RuleOrder,
                    RuleType = x.RuleType,
                    Severity = x.Severity,
                    IsPass = x.IsPass,
                    Message = x.Message
                }),
                JsonOptions);
            fieldValue.UpdatedAt = utcNow;
        }

        var fromStatus = submission.Status;
        submission.Status = SubmissionStatuses.AutoEvaluated;
        submission.AutoResult = submissionHasErrorFail ? SubmissionAutoResults.Fail : SubmissionAutoResults.Pass;
        submission.EvaluatedAt = utcNow;
        submission.UpdatedAt = utcNow;

        _dbContext.ApprovalLogs.Add(new ApprovalLog
        {
            SubmissionId = submission.Id,
            Action = ApprovalActions.AutoEvaluate,
            FromStatus = fromStatus,
            ToStatus = submission.Status,
            ActionByUserId = null,
            MetadataJson = JsonSerializer.Serialize(new { submission.AutoResult }, JsonOptions),
            ActionAt = utcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildWorkflowResultAsync(submission, cancellationToken);
    }

    public async Task<SubmissionWorkflowResult> ApproveAsync(
        ApproveSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.ReportSubmissions
            .SingleOrDefaultAsync(x => x.Id == request.SubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new WorkflowNotFoundException($"Submission `{request.SubmissionId}` was not found.");
        }

        EnsureSubmissionStatus(
            submission.Status,
            SubmissionStatuses.AutoEvaluated,
            "Only AUTO_EVALUATED submissions can be approved.");

        await EnsureActionUserExistsAsync(request.ActionByUserId, cancellationToken);

        if (string.Equals(submission.AutoResult, SubmissionAutoResults.Fail, StringComparison.Ordinal))
        {
            throw new WorkflowRuleViolationException("Submission cannot be approved because auto evaluation contains ERROR rule failures.");
        }

        if (!string.Equals(request.ManagerResult, SubmissionAutoResults.Pass, StringComparison.Ordinal) &&
            !string.Equals(request.ManagerResult, SubmissionAutoResults.Fail, StringComparison.Ordinal))
        {
            throw new WorkflowRuleViolationException("Manager result for approve must be PASS or FAIL.");
        }

        var utcNow = DateTime.UtcNow;
        var fromStatus = submission.Status;
        submission.Status = SubmissionStatuses.Approved;
        submission.ManagerResult = request.ManagerResult;
        submission.ManagerNote = request.ManagerNote;
        submission.ApprovedByUserId = request.ActionByUserId;
        submission.ApprovedAt = utcNow;
        submission.UpdatedAt = utcNow;

        _dbContext.ApprovalLogs.Add(new ApprovalLog
        {
            SubmissionId = submission.Id,
            Action = ApprovalActions.Approve,
            FromStatus = fromStatus,
            ToStatus = submission.Status,
            ActionByUserId = request.ActionByUserId,
            Comment = request.ManagerNote,
            ActionAt = utcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildWorkflowResultAsync(submission, cancellationToken);
    }

    public async Task<SubmissionWorkflowResult> RejectAsync(
        RejectSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.ReportSubmissions
            .SingleOrDefaultAsync(x => x.Id == request.SubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new WorkflowNotFoundException($"Submission `{request.SubmissionId}` was not found.");
        }

        EnsureSubmissionStatus(
            submission.Status,
            SubmissionStatuses.AutoEvaluated,
            "Only AUTO_EVALUATED submissions can be rejected.");

        await EnsureActionUserExistsAsync(request.ActionByUserId, cancellationToken);

        var utcNow = DateTime.UtcNow;
        var fromStatus = submission.Status;
        submission.Status = SubmissionStatuses.Rejected;
        submission.ManagerResult = SubmissionAutoResults.Fail;
        submission.ManagerNote = request.ManagerNote;
        submission.ApprovedByUserId = request.ActionByUserId;
        submission.ApprovedAt = utcNow;
        submission.UpdatedAt = utcNow;

        _dbContext.ApprovalLogs.Add(new ApprovalLog
        {
            SubmissionId = submission.Id,
            Action = ApprovalActions.Reject,
            FromStatus = fromStatus,
            ToStatus = submission.Status,
            ActionByUserId = request.ActionByUserId,
            Comment = request.ManagerNote,
            ActionAt = utcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildWorkflowResultAsync(submission, cancellationToken);
    }

    public async Task<SubmissionWorkflowResult> ReopenAsync(
        ReopenSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var sourceSubmission = await _dbContext.ReportSubmissions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.SubmissionId, cancellationToken);

        if (sourceSubmission is null)
        {
            throw new WorkflowNotFoundException($"Submission `{request.SubmissionId}` was not found.");
        }

        var isAllowedSourceStatus =
            string.Equals(sourceSubmission.Status, SubmissionStatuses.Rejected, StringComparison.Ordinal) ||
            string.Equals(sourceSubmission.Status, SubmissionStatuses.Approved, StringComparison.Ordinal);

        if (!isAllowedSourceStatus)
        {
            throw new WorkflowRuleViolationException("Only REJECTED or APPROVED submissions can be reopened.");
        }

        await EnsureActionUserExistsAsync(request.ActionByUserId, cancellationToken);

        var utcNow = DateTime.UtcNow;
        var draft = new ReportSubmission
        {
            SubmissionNo = await GenerateSubmissionNoAsync(cancellationToken),
            TemplateVersionId = sourceSubmission.TemplateVersionId,
            ReportDate = sourceSubmission.ReportDate,
            CreatedByUserId = sourceSubmission.CreatedByUserId,
            PerformedByText = sourceSubmission.PerformedByText,
            Status = SubmissionStatuses.Draft,
            AutoResult = SubmissionAutoResults.Pending,
            ManagerResult = SubmissionAutoResults.Pending,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.ReportSubmissions.Add(draft);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var sourceFieldValues = await _dbContext.ReportFieldValues
            .AsNoTracking()
            .Where(x => x.SubmissionId == sourceSubmission.Id)
            .ToListAsync(cancellationToken);

        foreach (var source in sourceFieldValues)
        {
            _dbContext.ReportFieldValues.Add(new ReportFieldValue
            {
                SubmissionId = draft.Id,
                FieldId = source.FieldId,
                ValueText = source.ValueText,
                ValueNumber = source.ValueNumber,
                ValueDate = source.ValueDate,
                ValueDateTime = source.ValueDateTime,
                ValueBool = source.ValueBool,
                NormalizedValue = source.NormalizedValue,
                AutoResult = SubmissionAutoResults.Pending,
                EvaluationNote = null,
                RuleSnapshotJson = null,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            });
        }

        _dbContext.ApprovalLogs.Add(new ApprovalLog
        {
            SubmissionId = draft.Id,
            Action = ApprovalActions.Reopen,
            FromStatus = sourceSubmission.Status,
            ToStatus = SubmissionStatuses.Draft,
            ActionByUserId = request.ActionByUserId,
            Comment = request.Reason,
            MetadataJson = JsonSerializer.Serialize(new { sourceSubmissionId = sourceSubmission.Id }, JsonOptions),
            ActionAt = utcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildWorkflowResultAsync(draft, cancellationToken);
    }

    private async Task EnsureActionUserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Users.AnyAsync(x => x.Id == userId, cancellationToken);
        if (!exists)
        {
            throw new WorkflowNotFoundException($"User `{userId}` was not found.");
        }
    }

    private async Task<SubmissionWorkflowResult> BuildWorkflowResultAsync(
        ReportSubmission submission,
        CancellationToken cancellationToken)
    {
        var logs = await _dbContext.ApprovalLogs
            .AsNoTracking()
            .Where(x => x.SubmissionId == submission.Id)
            .OrderBy(x => x.ActionAt)
            .ThenBy(x => x.Id)
            .Select(x => new SubmissionWorkflowLogItem
            {
                LogId = x.Id,
                Action = x.Action,
                FromStatus = x.FromStatus,
                ToStatus = x.ToStatus,
                ActionByUserId = x.ActionByUserId,
                Comment = x.Comment,
                MetadataJson = x.MetadataJson,
                ActionAt = x.ActionAt
            })
            .ToListAsync(cancellationToken);

        return new SubmissionWorkflowResult
        {
            SubmissionId = submission.Id,
            SubmissionNo = submission.SubmissionNo,
            TemplateVersionId = submission.TemplateVersionId,
            ReportDate = submission.ReportDate,
            CreatedByUserId = submission.CreatedByUserId,
            PerformedByText = submission.PerformedByText,
            Status = submission.Status,
            AutoResult = submission.AutoResult,
            ManagerResult = submission.ManagerResult,
            ManagerNote = submission.ManagerNote,
            ApprovedByUserId = submission.ApprovedByUserId,
            SubmittedAt = submission.SubmittedAt,
            EvaluatedAt = submission.EvaluatedAt,
            ApprovedAt = submission.ApprovedAt,
            CreatedAt = submission.CreatedAt,
            UpdatedAt = submission.UpdatedAt,
            Logs = logs
        };
    }

    private static void EnsureSubmissionStatus(string currentStatus, string expectedStatus, string message)
    {
        if (!string.Equals(currentStatus, expectedStatus, StringComparison.Ordinal))
        {
            throw new WorkflowRuleViolationException(message);
        }
    }

    private static bool HasEffectiveValue(string dataType, ReportFieldValue value)
    {
        return dataType switch
        {
            "NUMBER" => value.ValueNumber.HasValue,
            "TEXT" => !string.IsNullOrWhiteSpace(value.ValueText),
            "SELECT" => !string.IsNullOrWhiteSpace(value.ValueText),
            "DATE" => value.ValueDate.HasValue,
            "DATETIME" => value.ValueDateTime.HasValue,
            "BOOLEAN" => value.ValueBool.HasValue,
            _ => !string.IsNullOrWhiteSpace(value.ValueText) ||
                 value.ValueNumber.HasValue ||
                 value.ValueDate.HasValue ||
                 value.ValueDateTime.HasValue ||
                 value.ValueBool.HasValue
        };
    }

    private static RuleEvaluationResult EvaluateRule(TemplateField field, ReportFieldValue value, FieldRule rule)
    {
        var severity = rule.Severity;
        var defaultMessage = !string.IsNullOrWhiteSpace(rule.FailMessage)
            ? rule.FailMessage!
            : $"Field `{field.FieldCode}` does not satisfy rule `{rule.RuleType}`.";

        var isPass = rule.RuleType switch
        {
            RuleTypes.Range => EvaluateRange(value.ValueNumber, rule.MinValue, rule.MaxValue),
            RuleTypes.LessThan => EvaluateLt(value.ValueNumber, rule.ThresholdValue),
            RuleTypes.LessThanOrEqual => EvaluateLte(value.ValueNumber, rule.ThresholdValue),
            RuleTypes.GreaterThan => EvaluateGt(value.ValueNumber, rule.ThresholdValue),
            RuleTypes.GreaterThanOrEqual => EvaluateGte(value.ValueNumber, rule.ThresholdValue),
            RuleTypes.Regex => EvaluateRegex(value.ValueText, rule.ExpectedText),
            RuleTypes.InSet => EvaluateInSet(value.ValueText, rule.ExpectedText),
            _ => false
        };

        return new RuleEvaluationResult
        {
            RuleOrder = rule.RuleOrder,
            RuleType = rule.RuleType,
            Severity = severity,
            IsPass = isPass,
            Message = isPass ? null : defaultMessage
        };
    }

    private static bool EvaluateRange(decimal? value, decimal? minValue, decimal? maxValue)
    {
        if (!value.HasValue || !minValue.HasValue || !maxValue.HasValue)
        {
            return false;
        }

        return value.Value >= minValue.Value && value.Value <= maxValue.Value;
    }

    private static bool EvaluateLt(decimal? value, decimal? threshold)
    {
        return value.HasValue && threshold.HasValue && value.Value < threshold.Value;
    }

    private static bool EvaluateLte(decimal? value, decimal? threshold)
    {
        return value.HasValue && threshold.HasValue && value.Value <= threshold.Value;
    }

    private static bool EvaluateGt(decimal? value, decimal? threshold)
    {
        return value.HasValue && threshold.HasValue && value.Value > threshold.Value;
    }

    private static bool EvaluateGte(decimal? value, decimal? threshold)
    {
        return value.HasValue && threshold.HasValue && value.Value >= threshold.Value;
    }

    private static bool EvaluateRegex(string? valueText, string? pattern)
    {
        if (string.IsNullOrWhiteSpace(valueText) || string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(valueText, pattern);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool EvaluateInSet(string? valueText, string? expectedText)
    {
        if (string.IsNullOrWhiteSpace(valueText) || string.IsNullOrWhiteSpace(expectedText))
        {
            return false;
        }

        var set = expectedText
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return set.Contains(valueText.Trim());
    }

    private static string? NormalizeValue(SubmissionFieldValueInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.ValueText))
        {
            return input.ValueText.Trim();
        }

        if (input.ValueNumber.HasValue)
        {
            return input.ValueNumber.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (input.ValueDate.HasValue)
        {
            return input.ValueDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (input.ValueDateTime.HasValue)
        {
            return input.ValueDateTime.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        if (input.ValueBool.HasValue)
        {
            return input.ValueBool.Value ? "true" : "false";
        }

        return null;
    }

    private async Task<string> GenerateSubmissionNoAsync(CancellationToken cancellationToken)
    {
        var prefix = $"SUB-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var candidate = prefix;
        var sequence = 0;

        while (await _dbContext.ReportSubmissions.AnyAsync(x => x.SubmissionNo == candidate, cancellationToken))
        {
            sequence++;
            candidate = $"{prefix}-{sequence:000}";
        }

        return candidate;
    }

    private sealed class RuleEvaluationResult
    {
        public int RuleOrder { get; init; }
        public string RuleType { get; init; } = string.Empty;
        public string Severity { get; init; } = string.Empty;
        public bool IsPass { get; init; }
        public bool IsErrorSeverity => string.Equals(Severity, RuleSeverities.Error, StringComparison.Ordinal);
        public string? Message { get; init; }
    }

    private sealed class RuleSnapshotItem
    {
        public int RuleOrder { get; init; }
        public string RuleType { get; init; } = string.Empty;
        public string Severity { get; init; } = string.Empty;
        public bool IsPass { get; init; }
        public string? Message { get; init; }
    }
}
