using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Infrastructure.Data;
using ReportSystem.Web.Security;

namespace ReportSystem.Web.Controllers;

[ApiController]
[Route("api/submissions/{submissionId:long}")]
[Authorize]
public sealed class SubmissionArtifactsController : ControllerBase
{
    private readonly ReportSystemDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public SubmissionArtifactsController(
        ReportSystemDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [HttpGet("attachments")]
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> GetAttachments([FromRoute] long submissionId, CancellationToken cancellationToken)
    {
        var submissionExists = await _dbContext.ReportSubmissions
            .AsNoTracking()
            .AnyAsync(x => x.Id == submissionId, cancellationToken);

        if (!submissionExists)
        {
            return NotFound(new { message = $"Submission `{submissionId}` was not found." });
        }

        var attachments = await _dbContext.ReportAttachments
            .AsNoTracking()
            .Where(x => x.SubmissionId == submissionId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AttachmentResponse(
                x.Id,
                x.SubmissionId,
                x.FilePath,
                x.FileName,
                x.ContentType,
                x.FileSizeBytes,
                x.CapturedAt,
                x.UploadedByUserId,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(attachments);
    }

    [HttpPost("attachments/upload")]
    [Authorize(Roles = RoleGroups.EmployeeOrAdmin)]
    [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
    public async Task<IActionResult> UploadAttachment(
        [FromRoute] long submissionId,
        [FromForm] UploadAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { message = "Attachment file is required." });
        }

        var submission = await _dbContext.ReportSubmissions
            .SingleOrDefaultAsync(x => x.Id == submissionId, cancellationToken);
        if (submission is null)
        {
            return NotFound(new { message = $"Submission `{submissionId}` was not found." });
        }

        if (string.Equals(submission.Status, "APPROVED", StringComparison.Ordinal) ||
            string.Equals(submission.Status, "REJECTED", StringComparison.Ordinal))
        {
            return Conflict(new { message = "Cannot upload attachment for APPROVED or REJECTED submissions." });
        }

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is missing in authentication context." });
        }

        var uploadsRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var relativeDirectory = Path.Combine("uploads", "attachments", submissionId.ToString(CultureInfo.InvariantCulture));
        var absoluteDirectory = Path.Combine(uploadsRoot, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var originalFileName = Path.GetFileName(request.File.FileName);
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteDirectory, storedFileName);

        await using (var stream = System.IO.File.Create(absolutePath))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = Path.Combine(relativeDirectory, storedFileName).Replace("\\", "/");
        var attachment = new Domain.Entities.ReportAttachment
        {
            SubmissionId = submissionId,
            FilePath = relativePath,
            FileName = string.IsNullOrWhiteSpace(originalFileName) ? storedFileName : originalFileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            CapturedAt = request.CapturedAt?.ToUniversalTime(),
            UploadedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ReportAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AttachmentResponse(
            attachment.Id,
            attachment.SubmissionId,
            attachment.FilePath,
            attachment.FileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.CapturedAt,
            attachment.UploadedByUserId,
            attachment.CreatedAt));
    }

    [HttpDelete("attachments/{attachmentId:long}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteAttachment(
        [FromRoute] long submissionId,
        [FromRoute] long attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.ReportAttachments
            .SingleOrDefaultAsync(
                x => x.Id == attachmentId && x.SubmissionId == submissionId,
                cancellationToken);

        if (attachment is null)
        {
            return NotFound();
        }

        var uploadsRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var absolutePath = Path.Combine(uploadsRoot, attachment.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (System.IO.File.Exists(absolutePath))
        {
            System.IO.File.Delete(absolutePath);
        }

        _dbContext.ReportAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("export/excel")]
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> ExportExcel([FromRoute] long submissionId, CancellationToken cancellationToken)
    {
        var report = await BuildExportReportAsync(submissionId, cancellationToken);
        if (report is null)
        {
            return NotFound(new { message = $"Submission `{submissionId}` was not found." });
        }

        var bytes = BuildExcelBytes(report);
        var fileName = $"{report.SubmissionNo}.xls";
        return File(bytes, "application/vnd.ms-excel", fileName);
    }

    [HttpGet("export/pdf")]
    [Authorize(Roles = RoleGroups.AllRoles)]
    public async Task<IActionResult> ExportPdf([FromRoute] long submissionId, CancellationToken cancellationToken)
    {
        var report = await BuildExportReportAsync(submissionId, cancellationToken);
        if (report is null)
        {
            return NotFound(new { message = $"Submission `{submissionId}` was not found." });
        }

        var bytes = BuildPdfBytes(report);
        var fileName = $"{report.SubmissionNo}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    private async Task<ExportReport?> BuildExportReportAsync(long submissionId, CancellationToken cancellationToken)
    {
        var submission = await _dbContext.ReportSubmissions
            .AsNoTracking()
            .Where(x => x.Id == submissionId)
            .Select(x => new
            {
                x.Id,
                x.SubmissionNo,
                x.TemplateVersionId,
                x.ReportDate,
                x.Status,
                x.AutoResult,
                x.ManagerResult,
                x.ManagerNote,
                x.PerformedByText,
                x.CreatedByUserId,
                x.CreatedAt,
                x.SubmittedAt,
                x.EvaluatedAt,
                x.ApprovedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (submission is null)
        {
            return null;
        }

        var templateVersion = await _dbContext.ReportTemplateVersions
            .AsNoTracking()
            .Where(x => x.Id == submission.TemplateVersionId)
            .Select(x => new { x.Id, x.VersionNo, x.TemplateId })
            .SingleOrDefaultAsync(cancellationToken);
        if (templateVersion is null)
        {
            return null;
        }

        var template = await _dbContext.ReportTemplates
            .AsNoTracking()
            .Where(x => x.Id == templateVersion.TemplateId)
            .Select(x => new { x.TemplateName })
            .SingleOrDefaultAsync(cancellationToken);

        var creator = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == submission.CreatedByUserId)
            .Select(x => new { x.EmployeeCode, x.FullName })
            .SingleOrDefaultAsync(cancellationToken);

        var fieldRows = await (
            from field in _dbContext.TemplateFields.AsNoTracking()
            join value in _dbContext.ReportFieldValues.AsNoTracking()
                on new { SubmissionId = submission.Id, FieldId = field.Id }
                equals new { value.SubmissionId, value.FieldId } into valueJoin
            from value in valueJoin.DefaultIfEmpty()
            where field.TemplateVersionId == submission.TemplateVersionId
            orderby field.DisplayOrder
            select new ExportFieldRow(
                field.FieldLabel,
                value != null ? value.ValueText : null,
                value != null ? value.ValueNumber : null,
                value != null ? value.ValueDate : null,
                value != null ? value.ValueDateTime : null,
                value != null ? value.ValueBool : null))
            .ToListAsync(cancellationToken);

        return new ExportReport(
            submission.SubmissionNo,
            template?.TemplateName ?? $"Template {templateVersion.TemplateId}",
            templateVersion.VersionNo,
            submission.ReportDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            submission.Status,
            submission.AutoResult,
            submission.ManagerResult,
            submission.ManagerNote,
            creator is null ? submission.CreatedByUserId.ToString() : $"{creator.EmployeeCode} - {creator.FullName}",
            submission.PerformedByText,
            submission.CreatedAt,
            submission.SubmittedAt,
            submission.EvaluatedAt,
            submission.ApprovedAt,
            fieldRows);
    }

    private static byte[] BuildExcelBytes(ExportReport report)
    {
        static string Esc(string? value)
        {
            return (value ?? string.Empty)
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal);
        }

        static void AppendRow(StringBuilder sb, params string[] cells)
        {
            sb.Append("<Row>");
            foreach (var cell in cells)
            {
                sb.Append("<Cell><Data ss:Type=\"String\">");
                sb.Append(Esc(cell));
                sb.Append("</Data></Cell>");
            }
            sb.Append("</Row>");
        }

        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\"?>");
        sb.Append("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.Append("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" ");
        sb.Append("xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
        sb.Append("<Worksheet ss:Name=\"Submission\"><Table>");

        AppendRow(sb, "Submission No", report.SubmissionNo);
        AppendRow(sb, "Template", $"{report.TemplateName} - v{report.TemplateVersionNo}");
        AppendRow(sb, "Report Date", report.ReportDate);
        AppendRow(sb, "Status", report.Status);
        AppendRow(sb, "Auto Result", report.AutoResult);
        AppendRow(sb, "Manager Result", report.ManagerResult);
        AppendRow(sb, "Manager Note", report.ManagerNote ?? string.Empty);
        AppendRow(sb, "Created By", report.CreatedBy);
        AppendRow(sb, "Performed By", report.PerformedBy ?? string.Empty);
        AppendRow(sb, "Submitted At", report.SubmittedAt?.ToString("u", CultureInfo.InvariantCulture) ?? string.Empty);
        AppendRow(sb, "Evaluated At", report.EvaluatedAt?.ToString("u", CultureInfo.InvariantCulture) ?? string.Empty);
        AppendRow(sb, "Approved At", report.ApprovedAt?.ToString("u", CultureInfo.InvariantCulture) ?? string.Empty);
        AppendRow(sb, string.Empty, string.Empty);
        AppendRow(sb, "Label", "Value");

        foreach (var row in report.Fields)
        {
            AppendRow(sb, row.FieldLabel, GetFieldDisplayValue(row));
        }

        sb.Append("</Table></Worksheet></Workbook>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static byte[] BuildPdfBytes(ExportReport report)
    {
        var lines = new List<string>
        {
            "Submission Report",
            $"No: {report.SubmissionNo}",
            $"Template: {report.TemplateName} - v{report.TemplateVersionNo}",
            $"Date: {report.ReportDate}",
            $"Status: {report.Status} | Auto: {report.AutoResult} | Manager: {report.ManagerResult}",
            $"Created By: {report.CreatedBy}",
            $"Performed By: {report.PerformedBy ?? "-"}",
            $"Manager Note: {report.ManagerNote ?? "-"}",
            string.Empty,
            "Fields:"
        };

        lines.AddRange(report.Fields.Select(row =>
            $"{row.FieldLabel}: {GetFieldDisplayValue(row)}"));

        return BuildSimplePdf(lines);
    }

    private static byte[] BuildSimplePdf(IReadOnlyCollection<string> rawLines)
    {
        static string Sanitize(string value)
        {
            var chars = value.Select(c => c <= 127 ? c : '?').ToArray();
            return new string(chars)
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal);
        }

        var maxLines = 48;
        var lines = rawLines.Take(maxLines).ToArray();
        var contentBuilder = new StringBuilder();
        contentBuilder.Append("BT /F1 11 Tf 50 780 Td 14 TL ");
        for (var i = 0; i < lines.Length; i++)
        {
            contentBuilder.Append('(').Append(Sanitize(lines[i])).Append(") Tj");
            if (i < lines.Length - 1)
            {
                contentBuilder.Append(" T* ");
            }
        }

        contentBuilder.Append(" ET");
        var content = contentBuilder.ToString();
        var contentBytes = Encoding.ASCII.GetBytes(content);

        var objects = new[]
        {
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n",
            "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj\n",
            "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj\n",
            "4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj\n",
            $"5 0 obj << /Length {contentBytes.Length} >> stream\n{content}\nendstream endobj\n"
        };

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII, leaveOpen: true);

        writer.Write("%PDF-1.4\n");
        writer.Flush();

        var offsets = new List<long> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(ms.Position);
            writer.Write(obj);
            writer.Flush();
        }

        var xrefStart = ms.Position;
        writer.Write($"xref\n0 {offsets.Count}\n");
        writer.Write("0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++)
        {
            writer.Write($"{offsets[i]:0000000000} 00000 n \n");
        }

        writer.Write($"trailer << /Size {offsets.Count} /Root 1 0 R >>\n");
        writer.Write($"startxref\n{xrefStart}\n%%EOF");
        writer.Flush();

        return ms.ToArray();
    }

    private static string GetFieldDisplayValue(ExportFieldRow row)
    {
        if (!string.IsNullOrWhiteSpace(row.ValueText))
        {
            return row.ValueText!;
        }

        if (row.ValueNumber.HasValue)
        {
            return row.ValueNumber.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (row.ValueDate.HasValue)
        {
            return row.ValueDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (row.ValueDateTime.HasValue)
        {
            return row.ValueDateTime.Value.ToString("u", CultureInfo.InvariantCulture);
        }

        if (row.ValueBool.HasValue)
        {
            return row.ValueBool.Value ? "true" : "false";
        }

        return string.Empty;
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out userId);
    }

    public sealed class UploadAttachmentRequest
    {
        public IFormFile? File { get; set; }
        public DateTime? CapturedAt { get; set; }
    }

    public sealed record AttachmentResponse(
        long Id,
        long SubmissionId,
        string FilePath,
        string FileName,
        string? ContentType,
        long? FileSizeBytes,
        DateTime? CapturedAt,
        Guid UploadedByUserId,
        DateTime CreatedAt);

    private sealed record ExportReport(
        string SubmissionNo,
        string TemplateName,
        int TemplateVersionNo,
        string ReportDate,
        string Status,
        string AutoResult,
        string ManagerResult,
        string? ManagerNote,
        string CreatedBy,
        string? PerformedBy,
        DateTime CreatedAt,
        DateTime? SubmittedAt,
        DateTime? EvaluatedAt,
        DateTime? ApprovedAt,
        IReadOnlyCollection<ExportFieldRow> Fields);

    private sealed record ExportFieldRow(
        string FieldLabel,
        string? ValueText,
        decimal? ValueNumber,
        DateOnly? ValueDate,
        DateTime? ValueDateTime,
        bool? ValueBool);
}
