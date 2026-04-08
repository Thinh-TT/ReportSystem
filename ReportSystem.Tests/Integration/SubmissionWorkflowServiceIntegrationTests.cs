using Microsoft.EntityFrameworkCore;
using ReportSystem.Application.Services.Workflow;
using ReportSystem.Domain.Constants;
using ReportSystem.Infrastructure.Seed;
using ReportSystem.Infrastructure.Services;
using Xunit;

namespace ReportSystem.Tests.Integration;

public class SubmissionWorkflowServiceIntegrationTests
{
    [Fact]
    public async Task EndToEnd_ApproveFlow_ShouldReachApprovedAndWriteLogs()
    {
        await using var dbContext = TestDbContextFactory.Create();
        await MinimalDataSeeder.SeedAsync(dbContext);

        var workflowService = new SubmissionWorkflowService(dbContext);
        var adminUserId = await dbContext.Users
            .Where(x => x.EmployeeCode == "ADMIN001")
            .Select(x => x.Id)
            .SingleAsync();

        var templateVersionId = await (
            from template in dbContext.ReportTemplates
            join version in dbContext.ReportTemplateVersions on template.Id equals version.TemplateId
            where template.TemplateCode == "PH_METER_DAILY_CHECK" &&
                  version.Status == TemplateVersionStatuses.Published
            select version.Id)
            .SingleAsync();

        var draft = await workflowService.CreateDraftAsync(new CreateDraftSubmissionRequest
        {
            TemplateVersionId = templateVersionId,
            ReportDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedByUserId = adminUserId,
            PerformedByText = "QA tester"
        });

        var fields = await dbContext.TemplateFields
            .Where(x => x.TemplateVersionId == templateVersionId)
            .ToDictionaryAsync(x => x.FieldCode, x => x.Id);

        await workflowService.UpdateFieldValuesAsync(new UpdateSubmissionFieldValuesRequest
        {
            SubmissionId = draft.SubmissionId,
            FieldValues =
            [
                new SubmissionFieldValueInput { FieldId = fields["date"], ValueDate = DateOnly.FromDateTime(DateTime.UtcNow) },
                new SubmissionFieldValueInput { FieldId = fields["ph_1"], ValueNumber = 7.01m },
                new SubmissionFieldValueInput { FieldId = fields["ph_2"], ValueNumber = 7.02m },
                new SubmissionFieldValueInput { FieldId = fields["ph_3"], ValueNumber = 7.03m },
                new SubmissionFieldValueInput { FieldId = fields["slope"], ValueNumber = 100m }
            ]
        });

        var submitted = await workflowService.SubmitAsync(new SubmitSubmissionRequest
        {
            SubmissionId = draft.SubmissionId,
            ActionByUserId = adminUserId
        });

        Assert.Equal(SubmissionStatuses.Submitted, submitted.Status);
        Assert.NotNull(submitted.SubmittedAt);

        var evaluated = await workflowService.AutoEvaluateAsync(new AutoEvaluateSubmissionRequest
        {
            SubmissionId = draft.SubmissionId
        });

        Assert.Equal(SubmissionStatuses.AutoEvaluated, evaluated.Status);
        Assert.Equal(SubmissionAutoResults.Pass, evaluated.AutoResult);
        Assert.NotNull(evaluated.EvaluatedAt);

        var approved = await workflowService.ApproveAsync(new ApproveSubmissionRequest
        {
            SubmissionId = draft.SubmissionId,
            ActionByUserId = adminUserId,
            ManagerResult = SubmissionAutoResults.Pass,
            ManagerNote = "Looks good"
        });

        Assert.Equal(SubmissionStatuses.Approved, approved.Status);
        Assert.Equal(SubmissionAutoResults.Pass, approved.ManagerResult);
        Assert.NotNull(approved.ApprovedAt);
        Assert.Contains(approved.Logs, x => x.Action == ApprovalActions.Submit);
        Assert.Contains(approved.Logs, x => x.Action == ApprovalActions.AutoEvaluate);
        Assert.Contains(approved.Logs, x => x.Action == ApprovalActions.Approve);

        var actions = await dbContext.ApprovalLogs
            .Where(x => x.SubmissionId == draft.SubmissionId)
            .Select(x => x.Action)
            .ToListAsync();

        Assert.Contains(ApprovalActions.Submit, actions);
        Assert.Contains(ApprovalActions.AutoEvaluate, actions);
        Assert.Contains(ApprovalActions.Approve, actions);
    }

    [Fact]
    public async Task EndToEnd_RejectFlow_ShouldRejectAndBlockApproveOnAutoFail()
    {
        await using var dbContext = TestDbContextFactory.Create();
        await MinimalDataSeeder.SeedAsync(dbContext);

        var workflowService = new SubmissionWorkflowService(dbContext);
        var adminUserId = await dbContext.Users
            .Where(x => x.EmployeeCode == "ADMIN001")
            .Select(x => x.Id)
            .SingleAsync();

        var templateVersionId = await (
            from template in dbContext.ReportTemplates
            join version in dbContext.ReportTemplateVersions on template.Id equals version.TemplateId
            where template.TemplateCode == "DISTILLED_WATER_QUALITY_CHECK" &&
                  version.Status == TemplateVersionStatuses.Published
            select version.Id)
            .SingleAsync();

        var draft = await workflowService.CreateDraftAsync(new CreateDraftSubmissionRequest
        {
            TemplateVersionId = templateVersionId,
            ReportDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedByUserId = adminUserId,
            PerformedByText = "QA tester"
        });

        var fields = await dbContext.TemplateFields
            .Where(x => x.TemplateVersionId == templateVersionId)
            .ToDictionaryAsync(x => x.FieldCode, x => x.Id);

        await workflowService.UpdateFieldValuesAsync(new UpdateSubmissionFieldValuesRequest
        {
            SubmissionId = draft.SubmissionId,
            FieldValues =
            [
                new SubmissionFieldValueInput { FieldId = fields["date"], ValueDate = DateOnly.FromDateTime(DateTime.UtcNow) },
                new SubmissionFieldValueInput { FieldId = fields["batch"], ValueText = "BATCH-01" },
                new SubmissionFieldValueInput { FieldId = fields["ph"], ValueNumber = 6.5m },
                new SubmissionFieldValueInput { FieldId = fields["tpc"], ValueNumber = 100m },
                new SubmissionFieldValueInput { FieldId = fields["ec"], ValueNumber = 10m },
                new SubmissionFieldValueInput { FieldId = fields["chlorine"], ValueNumber = 0.05m }
            ]
        });

        await workflowService.SubmitAsync(new SubmitSubmissionRequest
        {
            SubmissionId = draft.SubmissionId,
            ActionByUserId = adminUserId
        });

        var evaluated = await workflowService.AutoEvaluateAsync(new AutoEvaluateSubmissionRequest
        {
            SubmissionId = draft.SubmissionId
        });

        Assert.Equal(SubmissionAutoResults.Fail, evaluated.AutoResult);

        await Assert.ThrowsAsync<WorkflowRuleViolationException>(() =>
            workflowService.ApproveAsync(new ApproveSubmissionRequest
            {
                SubmissionId = draft.SubmissionId,
                ActionByUserId = adminUserId,
                ManagerResult = SubmissionAutoResults.Pass
            }));

        var rejected = await workflowService.RejectAsync(new RejectSubmissionRequest
        {
            SubmissionId = draft.SubmissionId,
            ActionByUserId = adminUserId,
            ManagerNote = "TPC over threshold"
        });

        Assert.Equal(SubmissionStatuses.Rejected, rejected.Status);
        Assert.Equal(SubmissionAutoResults.Fail, rejected.ManagerResult);

        var reopened = await workflowService.ReopenAsync(new ReopenSubmissionRequest
        {
            SubmissionId = rejected.SubmissionId,
            ActionByUserId = adminUserId,
            Reason = "Need retest after correction"
        });

        Assert.Equal(SubmissionStatuses.Draft, reopened.Status);
        Assert.Equal(SubmissionAutoResults.Pending, reopened.AutoResult);
        Assert.Equal(SubmissionAutoResults.Pending, reopened.ManagerResult);
        Assert.Contains(reopened.Logs, x => x.Action == ApprovalActions.Reopen);

        var sourceFieldValues = await dbContext.ReportFieldValues
            .Where(x => x.SubmissionId == rejected.SubmissionId)
            .ToListAsync();

        var reopenedFieldValues = await dbContext.ReportFieldValues
            .Where(x => x.SubmissionId == reopened.SubmissionId)
            .ToListAsync();

        Assert.Equal(sourceFieldValues.Count, reopenedFieldValues.Count);
    }
}
