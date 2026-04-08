using Microsoft.EntityFrameworkCore;
using ReportSystem.Domain.Constants;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Data;

public class ReportSystemDbContext : DbContext
{
    public ReportSystemDbContext(DbContextOptions<ReportSystemDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();

    public DbSet<ReportTemplateVersion> ReportTemplateVersions => Set<ReportTemplateVersion>();

    public DbSet<TemplateField> TemplateFields => Set<TemplateField>();

    public DbSet<FieldRule> FieldRules => Set<FieldRule>();

    public DbSet<ReportSubmission> ReportSubmissions => Set<ReportSubmission>();

    public DbSet<ReportFieldValue> ReportFieldValues => Set<ReportFieldValue>();

    public DbSet<ReportAttachment> ReportAttachments => Set<ReportAttachment>();

    public DbSet<ApprovalLog> ApprovalLogs => Set<ApprovalLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportSystemDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforceBusinessRulesAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(true, cancellationToken);
    }
    
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await EnforceBusinessRulesAsync(cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    
    private async Task EnforceBusinessRulesAsync(CancellationToken cancellationToken)
    {
        await EnforceTemplateMutationRulesAsync(cancellationToken);
        await EnforceSubmissionStateMachineRulesAsync(cancellationToken);
        await EnforceFieldValueMutationRulesAsync(cancellationToken);
    }
    
    private async Task EnforceTemplateMutationRulesAsync(CancellationToken cancellationToken)
    {
        var changedTemplateVersionIds = ChangeTracker.Entries<TemplateField>()
            .Where(x => x.State is EntityState.Modified or EntityState.Deleted)
            .Select(x => x.Entity.TemplateVersionId)
            .Where(x => x > 0)
            .ToHashSet();
    
        var changedRuleFieldIds = ChangeTracker.Entries<FieldRule>()
            .Where(x => x.State is EntityState.Modified or EntityState.Deleted)
            .Select(x => x.Entity.FieldId)
            .Where(x => x > 0)
            .ToArray();
    
        if (changedRuleFieldIds.Length > 0)
        {
            var ruleTemplateVersionIds = await TemplateFields
                .AsNoTracking()
                .Where(x => changedRuleFieldIds.Contains(x.Id))
                .Select(x => x.TemplateVersionId)
                .Distinct()
                .ToListAsync(cancellationToken);
    
            foreach (var versionId in ruleTemplateVersionIds)
            {
                changedTemplateVersionIds.Add(versionId);
            }
        }
    
        if (changedTemplateVersionIds.Count == 0)
        {
            return;
        }
    
        var hasLockedVersion = await ReportTemplateVersions
            .AsNoTracking()
            .AnyAsync(
                x => changedTemplateVersionIds.Contains(x.Id) &&
                     x.Status == TemplateVersionStatuses.Published &&
                     ReportSubmissions.Any(s => s.TemplateVersionId == x.Id),
                cancellationToken);
    
        if (hasLockedVersion)
        {
            throw new InvalidOperationException(
                "Cannot modify or delete template fields/rules of a PUBLISHED template version that already has submissions.");
        }
    }
    
    private Task EnforceSubmissionStateMachineRulesAsync(CancellationToken cancellationToken)
    {
        var addedSubmissionWithInvalidStatus = ChangeTracker.Entries<ReportSubmission>()
            .FirstOrDefault(x =>
                x.State == EntityState.Added &&
                !string.Equals(x.Entity.Status, SubmissionStatuses.Draft, StringComparison.Ordinal));
    
        if (addedSubmissionWithInvalidStatus is not null)
        {
            throw new InvalidOperationException("New submissions must start with DRAFT status.");
        }
    
        var invalidTransition = ChangeTracker.Entries<ReportSubmission>()
            .Where(x => x.State == EntityState.Modified && x.Property(y => y.Status).IsModified)
            .Select(x => new
            {
                Entry = x,
                From = x.OriginalValues.GetValue<string>(nameof(ReportSubmission.Status)),
                To = x.CurrentValues.GetValue<string>(nameof(ReportSubmission.Status))
            })
            .FirstOrDefault(x => !IsAllowedSubmissionTransition(x.From, x.To));
    
        if (invalidTransition is not null)
        {
            throw new InvalidOperationException(
                $"Invalid submission status transition `{invalidTransition.From}` -> `{invalidTransition.To}`.");
        }
    
        return Task.CompletedTask;
    }
    
    private async Task EnforceFieldValueMutationRulesAsync(CancellationToken cancellationToken)
    {
        var changedFieldValueEntries = ChangeTracker.Entries<ReportFieldValue>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
    
        if (changedFieldValueEntries.Count == 0)
        {
            return;
        }
    
        var submissionIds = changedFieldValueEntries
            .Select(x => x.Entity.SubmissionId)
            .Where(x => x > 0)
            .Distinct()
            .ToArray();
    
        if (submissionIds.Length == 0)
        {
            return;
        }

        var statusTransitions = ChangeTracker.Entries<ReportSubmission>()
            .Where(x => x.State == EntityState.Modified && x.Property(y => y.Status).IsModified)
            .ToDictionary(
                x => x.Entity.Id,
                x => new
                {
                    From = x.OriginalValues.GetValue<string>(nameof(ReportSubmission.Status)),
                    To = x.CurrentValues.GetValue<string>(nameof(ReportSubmission.Status))
                });
    
        var trackedStatuses = ChangeTracker.Entries<ReportSubmission>()
            .Where(x => x.Entity.Id > 0)
            .Select(x => new { x.Entity.Id, x.Entity.Status })
            .ToDictionary(x => x.Id, x => x.Status);
    
        var missingSubmissionIds = submissionIds.Where(x => !trackedStatuses.ContainsKey(x)).ToArray();
        if (missingSubmissionIds.Length > 0)
        {
            var dbStatuses = await ReportSubmissions
                .AsNoTracking()
                .Where(x => missingSubmissionIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Status })
                .ToListAsync(cancellationToken);
    
            foreach (var status in dbStatuses)
            {
                trackedStatuses[status.Id] = status.Status;
            }
        }

        var unresolvedSubmissionIds = submissionIds.Where(x => !trackedStatuses.ContainsKey(x)).ToArray();
        if (unresolvedSubmissionIds.Length > 0)
        {
            throw new InvalidOperationException(
                $"Submission not found for report_field_values change: {string.Join(", ", unresolvedSubmissionIds)}.");
        }
    
        var submissionNotInDraft = trackedStatuses
            .Where(x => submissionIds.Contains(x.Key) && !string.Equals(x.Value, SubmissionStatuses.Draft, StringComparison.Ordinal))
            .Select(x => x.Key)
            .ToArray();

        foreach (var submissionId in submissionNotInDraft)
        {
            var isAutoEvaluateTransition = statusTransitions.TryGetValue(submissionId, out var transition)
                                         && string.Equals(transition.From, SubmissionStatuses.Submitted, StringComparison.Ordinal)
                                         && string.Equals(transition.To, SubmissionStatuses.AutoEvaluated, StringComparison.Ordinal);

            if (!isAutoEvaluateTransition)
            {
                throw new InvalidOperationException(
                    $"Cannot modify report_field_values because submission `{submissionId}` is not in DRAFT status.");
            }

            var violatesAutoEvaluateMutation = changedFieldValueEntries
                .Where(x => x.Entity.SubmissionId == submissionId)
                .Any(x => ViolatesAutoEvaluateFieldValueMutationRule(x));

            if (violatesAutoEvaluateMutation)
            {
                throw new InvalidOperationException(
                    $"Cannot change raw field input values for submission `{submissionId}` outside DRAFT status.");
            }
        }
    
        var fieldIds = changedFieldValueEntries
            .Select(x => x.Entity.FieldId)
            .Where(x => x > 0)
            .Distinct()
            .ToArray();
    
        if (fieldIds.Length == 0)
        {
            return;
        }
    
        var validPairs = await (
            from submission in ReportSubmissions.AsNoTracking()
            join field in TemplateFields.AsNoTracking() on submission.TemplateVersionId equals field.TemplateVersionId
            where submissionIds.Contains(submission.Id) && fieldIds.Contains(field.Id)
            select new { SubmissionId = submission.Id, FieldId = field.Id })
            .ToListAsync(cancellationToken);
    
        var validPairSet = validPairs.Select(x => (x.SubmissionId, x.FieldId)).ToHashSet();
    
        var invalidFieldMapping = changedFieldValueEntries
            .Select(x => (x.Entity.SubmissionId, x.Entity.FieldId))
            .FirstOrDefault(x => x.SubmissionId > 0 && x.FieldId > 0 && !validPairSet.Contains(x));
    
        if (invalidFieldMapping != default)
        {
            throw new InvalidOperationException(
                $"Field `{invalidFieldMapping.FieldId}` does not belong to submission `{invalidFieldMapping.SubmissionId}` template version.");
        }
    }
    
    private static bool IsAllowedSubmissionTransition(string fromStatus, string toStatus)
    {
        if (string.Equals(fromStatus, toStatus, StringComparison.Ordinal))
        {
            return true;
        }
    
        return fromStatus switch
        {
            SubmissionStatuses.Draft => string.Equals(toStatus, SubmissionStatuses.Submitted, StringComparison.Ordinal),
            SubmissionStatuses.Submitted => string.Equals(toStatus, SubmissionStatuses.AutoEvaluated, StringComparison.Ordinal),
            SubmissionStatuses.AutoEvaluated => string.Equals(toStatus, SubmissionStatuses.Approved, StringComparison.Ordinal) ||
                                                string.Equals(toStatus, SubmissionStatuses.Rejected, StringComparison.Ordinal),
            _ => false
        };
    }

    private static bool ViolatesAutoEvaluateFieldValueMutationRule(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ReportFieldValue> entry)
    {
        if (entry.State == EntityState.Deleted)
        {
            return true;
        }

        if (entry.State == EntityState.Added)
        {
            return entry.Entity.ValueText is not null ||
                   entry.Entity.ValueNumber.HasValue ||
                   entry.Entity.ValueDate.HasValue ||
                   entry.Entity.ValueDateTime.HasValue ||
                   entry.Entity.ValueBool.HasValue ||
                   entry.Entity.NormalizedValue is not null;
        }

        return entry.Property(x => x.ValueText).IsModified ||
               entry.Property(x => x.ValueNumber).IsModified ||
               entry.Property(x => x.ValueDate).IsModified ||
               entry.Property(x => x.ValueDateTime).IsModified ||
               entry.Property(x => x.ValueBool).IsModified ||
               entry.Property(x => x.NormalizedValue).IsModified ||
               entry.Property(x => x.FieldId).IsModified ||
               entry.Property(x => x.SubmissionId).IsModified;
    }
}

