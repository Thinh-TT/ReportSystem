using Microsoft.EntityFrameworkCore;
using ReportSystem.Domain.Constants;
using ReportSystem.Domain.Entities;
using Xunit;

namespace ReportSystem.Tests.Smoke;

public class DbCrudSmokeTests
{
    [Fact]
    public async Task Crud_AllTables_ShouldSupportBasicCreateReadUpdateDelete()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var utcNow = DateTime.UtcNow;

        var role = new Role
        {
            Code = "QA_ROLE",
            Name = "Quality Role"
        };
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();

        role.Name = "Quality Role Updated";
        await dbContext.SaveChangesAsync();
        Assert.Equal("Quality Role Updated", (await dbContext.Roles.SingleAsync(x => x.Code == "QA_ROLE")).Name);

        var user = new User
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "QA001",
            FullName = "QA User",
            Email = "qa001@reportsystem.local",
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        user.FullName = "QA User Updated";
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        };
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();

        var template = new ReportTemplate
        {
            TemplateCode = "QA_TEMPLATE",
            TemplateName = "QA Template",
            Description = "Template for smoke CRUD test",
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
        dbContext.ReportTemplates.Add(template);
        await dbContext.SaveChangesAsync();

        template.TemplateName = "QA Template Updated";
        template.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var version = new ReportTemplateVersion
        {
            TemplateId = template.Id,
            VersionNo = 1,
            Status = TemplateVersionStatuses.Draft,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
        dbContext.ReportTemplateVersions.Add(version);
        await dbContext.SaveChangesAsync();

        var field = new TemplateField
        {
            TemplateVersionId = version.Id,
            FieldCode = "temperature",
            FieldLabel = "Temperature",
            DataType = "NUMBER",
            IsRequired = true,
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
        dbContext.TemplateFields.Add(field);
        await dbContext.SaveChangesAsync();

        field.FieldLabel = "Temperature Updated";
        field.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var rule = new FieldRule
        {
            FieldId = field.Id,
            RuleOrder = 1,
            RuleType = RuleTypes.Range,
            MinValue = 2m,
            MaxValue = 8m,
            Severity = RuleSeverities.Error,
            FailMessage = "Temperature out of range",
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
        dbContext.FieldRules.Add(rule);
        await dbContext.SaveChangesAsync();

        rule.MaxValue = 9m;
        rule.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var submission = new ReportSubmission
        {
            SubmissionNo = "SMOKE-0001",
            TemplateVersionId = version.Id,
            ReportDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedByUserId = user.Id,
            PerformedByText = "QA User",
            Status = SubmissionStatuses.Draft,
            AutoResult = SubmissionAutoResults.Pending,
            ManagerResult = SubmissionAutoResults.Pending,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
        dbContext.ReportSubmissions.Add(submission);
        await dbContext.SaveChangesAsync();

        submission.PerformedByText = "QA User Updated";
        submission.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var fieldValue = new ReportFieldValue
        {
            SubmissionId = submission.Id,
            FieldId = field.Id,
            ValueNumber = 5m,
            AutoResult = SubmissionAutoResults.Pending,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
        dbContext.ReportFieldValues.Add(fieldValue);
        await dbContext.SaveChangesAsync();

        fieldValue.ValueNumber = 6m;
        fieldValue.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var attachment = new ReportAttachment
        {
            SubmissionId = submission.Id,
            FilePath = "/tmp/qa-proof.jpg",
            FileName = "qa-proof.jpg",
            ContentType = "image/jpeg",
            FileSizeBytes = 128,
            UploadedByUserId = user.Id,
            CreatedAt = utcNow
        };
        dbContext.ReportAttachments.Add(attachment);
        await dbContext.SaveChangesAsync();

        attachment.FileName = "qa-proof-updated.jpg";
        await dbContext.SaveChangesAsync();

        var log = new ApprovalLog
        {
            SubmissionId = submission.Id,
            Action = ApprovalActions.Submit,
            FromStatus = SubmissionStatuses.Draft,
            ToStatus = SubmissionStatuses.Submitted,
            ActionByUserId = user.Id,
            Comment = "Smoke log",
            ActionAt = utcNow
        };
        dbContext.ApprovalLogs.Add(log);
        await dbContext.SaveChangesAsync();

        log.Comment = "Smoke log updated";
        await dbContext.SaveChangesAsync();

        Assert.True(await dbContext.Users.AnyAsync(x => x.Id == user.Id));
        Assert.True(await dbContext.Roles.AnyAsync(x => x.Id == role.Id));
        Assert.True(await dbContext.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id));
        Assert.True(await dbContext.ReportTemplates.AnyAsync(x => x.Id == template.Id));
        Assert.True(await dbContext.ReportTemplateVersions.AnyAsync(x => x.Id == version.Id));
        Assert.True(await dbContext.TemplateFields.AnyAsync(x => x.Id == field.Id));
        Assert.True(await dbContext.FieldRules.AnyAsync(x => x.Id == rule.Id));
        Assert.True(await dbContext.ReportSubmissions.AnyAsync(x => x.Id == submission.Id));
        Assert.True(await dbContext.ReportFieldValues.AnyAsync(x => x.Id == fieldValue.Id));
        Assert.True(await dbContext.ReportAttachments.AnyAsync(x => x.Id == attachment.Id));
        Assert.True(await dbContext.ApprovalLogs.AnyAsync(x => x.Id == log.Id));

        dbContext.ReportAttachments.Remove(attachment);
        dbContext.ApprovalLogs.Remove(log);
        dbContext.ReportFieldValues.Remove(fieldValue);
        dbContext.ReportSubmissions.Remove(submission);
        dbContext.UserRoles.Remove(userRole);
        dbContext.FieldRules.Remove(rule);
        dbContext.TemplateFields.Remove(field);
        dbContext.ReportTemplateVersions.Remove(version);
        dbContext.ReportTemplates.Remove(template);
        dbContext.Users.Remove(user);
        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync();

        Assert.False(await dbContext.Users.AnyAsync(x => x.Id == user.Id));
        Assert.False(await dbContext.Roles.AnyAsync(x => x.Id == role.Id));
        Assert.False(await dbContext.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id));
        Assert.False(await dbContext.ReportTemplates.AnyAsync(x => x.Id == template.Id));
        Assert.False(await dbContext.ReportTemplateVersions.AnyAsync(x => x.Id == version.Id));
        Assert.False(await dbContext.TemplateFields.AnyAsync(x => x.Id == field.Id));
        Assert.False(await dbContext.FieldRules.AnyAsync(x => x.Id == rule.Id));
        Assert.False(await dbContext.ReportSubmissions.AnyAsync(x => x.Id == submission.Id));
        Assert.False(await dbContext.ReportFieldValues.AnyAsync(x => x.Id == fieldValue.Id));
        Assert.False(await dbContext.ReportAttachments.AnyAsync(x => x.Id == attachment.Id));
        Assert.False(await dbContext.ApprovalLogs.AnyAsync(x => x.Id == log.Id));
    }
}
