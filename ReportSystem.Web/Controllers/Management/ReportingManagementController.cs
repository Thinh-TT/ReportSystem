using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Domain.Entities;
using ReportSystem.Infrastructure.Data;
using ReportSystem.Web.Security;

namespace ReportSystem.Web.Controllers.Management;

[ApiController]
[Route("api/management")]
[Authorize]
public sealed class ReportingManagementController : ControllerBase
{
    private readonly ReportSystemDbContext _dbContext;

    public ReportingManagementController(ReportSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("report-submissions")]
    [Authorize(Roles = RoleGroups.ManagerOrAdmin)]
    public async Task<IActionResult> GetSubmissions(CancellationToken cancellationToken)
    {
        var submissions = await _dbContext.ReportSubmissions
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(submissions.Select(MapSubmission));
    }

    [HttpGet("report-submissions/{id:long}")]
    [Authorize(Roles = RoleGroups.ManagerOrAdmin)]
    public async Task<IActionResult> GetSubmission([FromRoute] long id, CancellationToken cancellationToken)
    {
        var submission = await _dbContext.ReportSubmissions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return submission is null ? NotFound() : Ok(MapSubmission(submission));
    }

    [HttpPost("report-submissions")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateSubmission(
        [FromBody] ReportSubmissionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var submission = new ReportSubmission
        {
            SubmissionNo = request.SubmissionNo.Trim(),
            TemplateVersionId = request.TemplateVersionId,
            ReportDate = request.ReportDate,
            CreatedByUserId = request.CreatedByUserId,
            PerformedByText = request.PerformedByText?.Trim(),
            Status = request.Status.Trim(),
            AutoResult = request.AutoResult.Trim(),
            ManagerResult = request.ManagerResult.Trim(),
            ManagerNote = request.ManagerNote?.Trim(),
            ApprovedByUserId = request.ApprovedByUserId,
            ApprovedAt = ToUtc(request.ApprovedAt),
            SubmittedAt = ToUtc(request.SubmittedAt),
            EvaluatedAt = ToUtc(request.EvaluatedAt),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.ReportSubmissions.Add(submission);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, MapSubmission(submission));
    }

    [HttpPut("report-submissions/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateSubmission(
        [FromRoute] long id,
        [FromBody] ReportSubmissionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var submission = await _dbContext.ReportSubmissions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (submission is null)
        {
            return NotFound();
        }

        submission.SubmissionNo = request.SubmissionNo.Trim();
        submission.TemplateVersionId = request.TemplateVersionId;
        submission.ReportDate = request.ReportDate;
        submission.CreatedByUserId = request.CreatedByUserId;
        submission.PerformedByText = request.PerformedByText?.Trim();
        submission.Status = request.Status.Trim();
        submission.AutoResult = request.AutoResult.Trim();
        submission.ManagerResult = request.ManagerResult.Trim();
        submission.ManagerNote = request.ManagerNote?.Trim();
        submission.ApprovedByUserId = request.ApprovedByUserId;
        submission.ApprovedAt = ToUtc(request.ApprovedAt);
        submission.SubmittedAt = ToUtc(request.SubmittedAt);
        submission.EvaluatedAt = ToUtc(request.EvaluatedAt);
        submission.UpdatedAt = DateTime.UtcNow;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapSubmission(submission));
    }

    [HttpDelete("report-submissions/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteSubmission([FromRoute] long id, CancellationToken cancellationToken)
    {
        var submission = await _dbContext.ReportSubmissions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (submission is null)
        {
            return NotFound();
        }

        _dbContext.ReportSubmissions.Remove(submission);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    [HttpGet("report-field-values")]
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> GetFieldValues(CancellationToken cancellationToken)
    {
        var values = await _dbContext.ReportFieldValues
            .AsNoTracking()
            .OrderBy(x => x.SubmissionId)
            .ThenBy(x => x.FieldId)
            .ToListAsync(cancellationToken);

        return Ok(values.Select(MapFieldValue));
    }

    [HttpGet("report-field-values/{id:long}")]
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> GetFieldValue([FromRoute] long id, CancellationToken cancellationToken)
    {
        var fieldValue = await _dbContext.ReportFieldValues
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return fieldValue is null ? NotFound() : Ok(MapFieldValue(fieldValue));
    }

    [HttpPost("report-field-values")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateFieldValue(
        [FromBody] ReportFieldValueUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var fieldValue = new ReportFieldValue
        {
            SubmissionId = request.SubmissionId,
            FieldId = request.FieldId,
            ValueText = request.ValueText,
            ValueNumber = request.ValueNumber,
            ValueDate = request.ValueDate,
            ValueDateTime = ToUtc(request.ValueDateTime),
            ValueBool = request.ValueBool,
            NormalizedValue = request.NormalizedValue,
            AutoResult = request.AutoResult.Trim(),
            EvaluationNote = request.EvaluationNote,
            RuleSnapshotJson = request.RuleSnapshotJson,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.ReportFieldValues.Add(fieldValue);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetFieldValue), new { id = fieldValue.Id }, MapFieldValue(fieldValue));
    }

    [HttpPut("report-field-values/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateFieldValue(
        [FromRoute] long id,
        [FromBody] ReportFieldValueUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var fieldValue = await _dbContext.ReportFieldValues.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (fieldValue is null)
        {
            return NotFound();
        }

        fieldValue.SubmissionId = request.SubmissionId;
        fieldValue.FieldId = request.FieldId;
        fieldValue.ValueText = request.ValueText;
        fieldValue.ValueNumber = request.ValueNumber;
        fieldValue.ValueDate = request.ValueDate;
        fieldValue.ValueDateTime = ToUtc(request.ValueDateTime);
        fieldValue.ValueBool = request.ValueBool;
        fieldValue.NormalizedValue = request.NormalizedValue;
        fieldValue.AutoResult = request.AutoResult.Trim();
        fieldValue.EvaluationNote = request.EvaluationNote;
        fieldValue.RuleSnapshotJson = request.RuleSnapshotJson;
        fieldValue.UpdatedAt = DateTime.UtcNow;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapFieldValue(fieldValue));
    }

    [HttpDelete("report-field-values/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteFieldValue([FromRoute] long id, CancellationToken cancellationToken)
    {
        var fieldValue = await _dbContext.ReportFieldValues.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (fieldValue is null)
        {
            return NotFound();
        }

        _dbContext.ReportFieldValues.Remove(fieldValue);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    [HttpGet("report-attachments")]
    [Authorize(Roles = RoleGroups.ManagerOrAdmin)]
    public async Task<IActionResult> GetAttachments(CancellationToken cancellationToken)
    {
        var attachments = await _dbContext.ReportAttachments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(attachments.Select(MapAttachment));
    }

    [HttpGet("report-attachments/{id:long}")]
    [Authorize(Roles = RoleGroups.ManagerOrAdmin)]
    public async Task<IActionResult> GetAttachment([FromRoute] long id, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.ReportAttachments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return attachment is null ? NotFound() : Ok(MapAttachment(attachment));
    }

    [HttpPost("report-attachments")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateAttachment(
        [FromBody] ReportAttachmentUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var attachment = new ReportAttachment
        {
            SubmissionId = request.SubmissionId,
            FilePath = request.FilePath,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            CapturedAt = ToUtc(request.CapturedAt),
            UploadedByUserId = request.UploadedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ReportAttachments.Add(attachment);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetAttachment), new { id = attachment.Id }, MapAttachment(attachment));
    }

    [HttpPut("report-attachments/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateAttachment(
        [FromRoute] long id,
        [FromBody] ReportAttachmentUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.ReportAttachments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (attachment is null)
        {
            return NotFound();
        }

        attachment.SubmissionId = request.SubmissionId;
        attachment.FilePath = request.FilePath;
        attachment.FileName = request.FileName;
        attachment.ContentType = request.ContentType;
        attachment.FileSizeBytes = request.FileSizeBytes;
        attachment.CapturedAt = ToUtc(request.CapturedAt);
        attachment.UploadedByUserId = request.UploadedByUserId;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapAttachment(attachment));
    }

    [HttpDelete("report-attachments/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteAttachment([FromRoute] long id, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.ReportAttachments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (attachment is null)
        {
            return NotFound();
        }

        _dbContext.ReportAttachments.Remove(attachment);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    [HttpGet("approval-logs")]
    [Authorize(Roles = RoleGroups.ManagerOrAdmin)]
    public async Task<IActionResult> GetApprovalLogs(CancellationToken cancellationToken)
    {
        var logs = await _dbContext.ApprovalLogs
            .AsNoTracking()
            .OrderByDescending(x => x.ActionAt)
            .ToListAsync(cancellationToken);

        return Ok(logs.Select(MapApprovalLog));
    }

    [HttpGet("approval-logs/{id:long}")]
    [Authorize(Roles = RoleGroups.ManagerOrAdmin)]
    public async Task<IActionResult> GetApprovalLog([FromRoute] long id, CancellationToken cancellationToken)
    {
        var log = await _dbContext.ApprovalLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return log is null ? NotFound() : Ok(MapApprovalLog(log));
    }

    [HttpPost("approval-logs")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateApprovalLog(
        [FromBody] ApprovalLogUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var log = new ApprovalLog
        {
            SubmissionId = request.SubmissionId,
            Action = request.Action.Trim(),
            FromStatus = request.FromStatus?.Trim(),
            ToStatus = request.ToStatus?.Trim(),
            ActionByUserId = request.ActionByUserId,
            Comment = request.Comment,
            MetadataJson = request.MetadataJson,
            ActionAt = ToUtc(request.ActionAt) ?? DateTime.UtcNow
        };

        _dbContext.ApprovalLogs.Add(log);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetApprovalLog), new { id = log.Id }, MapApprovalLog(log));
    }

    [HttpPut("approval-logs/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateApprovalLog(
        [FromRoute] long id,
        [FromBody] ApprovalLogUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var log = await _dbContext.ApprovalLogs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (log is null)
        {
            return NotFound();
        }

        log.SubmissionId = request.SubmissionId;
        log.Action = request.Action.Trim();
        log.FromStatus = request.FromStatus?.Trim();
        log.ToStatus = request.ToStatus?.Trim();
        log.ActionByUserId = request.ActionByUserId;
        log.Comment = request.Comment;
        log.MetadataJson = request.MetadataJson;
        log.ActionAt = ToUtc(request.ActionAt) ?? log.ActionAt;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapApprovalLog(log));
    }

    [HttpDelete("approval-logs/{id:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteApprovalLog([FromRoute] long id, CancellationToken cancellationToken)
    {
        var log = await _dbContext.ApprovalLogs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (log is null)
        {
            return NotFound();
        }

        _dbContext.ApprovalLogs.Remove(log);
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

    private static ReportSubmissionResponse MapSubmission(ReportSubmission submission)
    {
        return new ReportSubmissionResponse(
            submission.Id,
            submission.SubmissionNo,
            submission.TemplateVersionId,
            submission.ReportDate,
            submission.CreatedByUserId,
            submission.PerformedByText,
            submission.Status,
            submission.AutoResult,
            submission.ManagerResult,
            submission.ManagerNote,
            submission.ApprovedByUserId,
            submission.ApprovedAt,
            submission.SubmittedAt,
            submission.EvaluatedAt,
            submission.CreatedAt,
            submission.UpdatedAt);
    }

    private static ReportFieldValueResponse MapFieldValue(ReportFieldValue value)
    {
        return new ReportFieldValueResponse(
            value.Id,
            value.SubmissionId,
            value.FieldId,
            value.ValueText,
            value.ValueNumber,
            value.ValueDate,
            value.ValueDateTime,
            value.ValueBool,
            value.NormalizedValue,
            value.AutoResult,
            value.EvaluationNote,
            value.RuleSnapshotJson,
            value.CreatedAt,
            value.UpdatedAt);
    }

    private static ReportAttachmentResponse MapAttachment(ReportAttachment attachment)
    {
        return new ReportAttachmentResponse(
            attachment.Id,
            attachment.SubmissionId,
            attachment.FilePath,
            attachment.FileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.CapturedAt,
            attachment.UploadedByUserId,
            attachment.CreatedAt);
    }

    private static ApprovalLogResponse MapApprovalLog(ApprovalLog log)
    {
        return new ApprovalLogResponse(
            log.Id,
            log.SubmissionId,
            log.Action,
            log.FromStatus,
            log.ToStatus,
            log.ActionByUserId,
            log.Comment,
            log.MetadataJson,
            log.ActionAt);
    }

    public sealed record ReportSubmissionResponse(
        long Id,
        string SubmissionNo,
        long TemplateVersionId,
        DateOnly ReportDate,
        Guid CreatedByUserId,
        string? PerformedByText,
        string Status,
        string AutoResult,
        string ManagerResult,
        string? ManagerNote,
        Guid? ApprovedByUserId,
        DateTime? ApprovedAt,
        DateTime? SubmittedAt,
        DateTime? EvaluatedAt,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed class ReportSubmissionUpsertRequest
    {
        public string SubmissionNo { get; set; } = string.Empty;
        public long TemplateVersionId { get; set; }
        public DateOnly ReportDate { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? PerformedByText { get; set; }
        public string Status { get; set; } = "DRAFT";
        public string AutoResult { get; set; } = "PENDING";
        public string ManagerResult { get; set; } = "PENDING";
        public string? ManagerNote { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? EvaluatedAt { get; set; }
    }

    public sealed record ReportFieldValueResponse(
        long Id,
        long SubmissionId,
        long FieldId,
        string? ValueText,
        decimal? ValueNumber,
        DateOnly? ValueDate,
        DateTime? ValueDateTime,
        bool? ValueBool,
        string? NormalizedValue,
        string AutoResult,
        string? EvaluationNote,
        string? RuleSnapshotJson,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed class ReportFieldValueUpsertRequest
    {
        public long SubmissionId { get; set; }
        public long FieldId { get; set; }
        public string? ValueText { get; set; }
        public decimal? ValueNumber { get; set; }
        public DateOnly? ValueDate { get; set; }
        public DateTime? ValueDateTime { get; set; }
        public bool? ValueBool { get; set; }
        public string? NormalizedValue { get; set; }
        public string AutoResult { get; set; } = "PENDING";
        public string? EvaluationNote { get; set; }
        public string? RuleSnapshotJson { get; set; }
    }

    public sealed record ReportAttachmentResponse(
        long Id,
        long SubmissionId,
        string FilePath,
        string FileName,
        string? ContentType,
        long? FileSizeBytes,
        DateTime? CapturedAt,
        Guid UploadedByUserId,
        DateTime CreatedAt);

    public sealed class ReportAttachmentUpsertRequest
    {
        public long SubmissionId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long? FileSizeBytes { get; set; }
        public DateTime? CapturedAt { get; set; }
        public Guid UploadedByUserId { get; set; }
    }

    public sealed record ApprovalLogResponse(
        long Id,
        long SubmissionId,
        string Action,
        string? FromStatus,
        string? ToStatus,
        Guid? ActionByUserId,
        string? Comment,
        string? MetadataJson,
        DateTime ActionAt);

    public sealed class ApprovalLogUpsertRequest
    {
        public long SubmissionId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? FromStatus { get; set; }
        public string? ToStatus { get; set; }
        public Guid? ActionByUserId { get; set; }
        public string? Comment { get; set; }
        public string? MetadataJson { get; set; }
        public DateTime? ActionAt { get; set; }
    }
}
