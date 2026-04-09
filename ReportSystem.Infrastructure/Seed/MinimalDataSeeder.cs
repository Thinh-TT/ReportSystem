using Microsoft.EntityFrameworkCore;
using ReportSystem.Domain.Entities;
using ReportSystem.Infrastructure.Data;

namespace ReportSystem.Infrastructure.Seed;

public static class MinimalDataSeeder
{
    private const string AdminRoleCode = "ADMIN";
    private const string ManagerRoleCode = "MANAGER";
    private const string EmployeeRoleCode = "EMPLOYEE";

    private const string AdminEmployeeCode = "ADMIN001";
    private const string AdminFullName = "System Administrator";
    private const string AdminEmail = "admin@reportsystem.local";
    private static readonly Guid AdminUserId = Guid.Parse("a6f8a419-df67-4c66-bf3b-84b7438f2de2");

    private const string ManagerEmployeeCode = "MANAGER001";
    private const string ManagerFullName = "Default Manager";
    private const string ManagerEmail = "manager@reportsystem.local";
    private static readonly Guid ManagerUserId = Guid.Parse("2f27efa4-c5f2-4c68-b300-a58bd26dc0b3");

    private const string EmployeeEmployeeCode = "EMPLOYEE001";
    private const string EmployeeFullName = "Default Employee";
    private const string EmployeeEmail = "employee@reportsystem.local";
    private static readonly Guid EmployeeUserId = Guid.Parse("27de669f-d982-45c8-9621-7322f3114aad");

    private static readonly RoleSeed[] RoleSeeds =
    {
        new("EMPLOYEE", "Employee"),
        new("MANAGER", "Manager"),
        new("ADMIN", "Administrator")
    };

    private static readonly TemplateSeed[] TemplateSeeds =
    {
        new(
            "0403_PH_METER_DAILY_CHECK",
            "0403_PH Meter Daily Check",
            "Daily verification checklist for pH meter.",
            new[]
            {
                new FieldSeed("date", "Date", "DATE", null, true, 1, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("ph_1", "pH #1", "NUMBER", null, true, 2, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("ph_2", "pH #2", "NUMBER", null, true, 3, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("ph_3", "pH #3", "NUMBER", null, true, 4, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed(
                    "slope",
                    "Slope",
                    "NUMBER",
                    null,
                    true,
                    5,
                    null,
                    null,
                    new[]
                    {
                        new RuleSeed(1, "RANGE", 85m, 115m, null, null, "ERROR", "Slope must be between 85 and 115.")
                    }),
                new FieldSeed("clean", "Clean", "BOOLEAN", null, false, 6, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("conclusion", "Conclusion", "TEXT", null, false, 7, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("performed_by", "Performed By", "TEXT", null, false, 8, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("remark", "Remark", "TEXT", null, false, 9, null, null, Array.Empty<RuleSeed>())
            }),
        new(
            "0398_DISTILLED_WATER_QUALITY_CHECK",
            "0398_Distilled Water Quality Check",
            "Quality control checklist for distilled water.",
            new[]
            {
                new FieldSeed("date", "Date", "DATE", null, true, 1, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("batch", "Batch", "TEXT", null, true, 2, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed(
                    "ph",
                    "pH",
                    "NUMBER",
                    null,
                    true,
                    3,
                    null,
                    null,
                    new[]
                    {
                        new RuleSeed(1, "RANGE", 5.0m, 7.5m, null, null, "ERROR", "pH must be between 5.0 and 7.5.")
                    }),
                new FieldSeed(
                    "tpc",
                    "TPC (CFU/mL)",
                    "NUMBER",
                    "CFU/mL",
                    true,
                    4,
                    null,
                    null,
                    new[]
                    {
                        new RuleSeed(1, "LT", null, null, 100m, null, "ERROR", "TPC must be less than 100 CFU/mL.")
                    }),
                new FieldSeed(
                    "ec",
                    "EC",
                    "NUMBER",
                    "uS/cm",
                    true,
                    5,
                    null,
                    null,
                    new[]
                    {
                        new RuleSeed(1, "LT", null, null, 25m, null, "ERROR", "EC must be less than 25 uS/cm.")
                    }),
                new FieldSeed(
                    "chlorine",
                    "Chlorine",
                    "NUMBER",
                    "ppm",
                    true,
                    6,
                    null,
                    null,
                    new[]
                    {
                        new RuleSeed(1, "LT", null, null, 0.1m, null, "ERROR", "Chlorine must be less than 0.1 ppm.")
                    }),
                new FieldSeed("conclusion", "Conclusion", "TEXT", null, false, 7, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("performed_by", "Performed By", "TEXT", null, false, 8, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("remark", "Remark", "TEXT", null, false, 9, null, null, Array.Empty<RuleSeed>()),
                new FieldSeed("equipment", "Equipment", "TEXT", null, false, 10, null, null, Array.Empty<RuleSeed>())
            })
    };

    public static async Task SeedAsync(ReportSystemDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        await SeedRolesAsync(dbContext, cancellationToken);
        var adminUser = await SeedAdminUserAsync(dbContext, utcNow, cancellationToken);
        var managerUser = await SeedManagerUserAsync(dbContext, utcNow, cancellationToken);
        var employeeUser = await SeedEmployeeUserAsync(dbContext, utcNow, cancellationToken);
        await SeedAdminRoleAsync(dbContext, adminUser.Id, cancellationToken);
        await SeedUserRoleAsync(dbContext, managerUser.Id, ManagerRoleCode, cancellationToken);
        await SeedUserRoleAsync(dbContext, employeeUser.Id, EmployeeRoleCode, cancellationToken);
        await SeedTemplatesAsync(dbContext, adminUser.Id, utcNow, cancellationToken);
    }

    private static async Task SeedRolesAsync(ReportSystemDbContext dbContext, CancellationToken cancellationToken)
    {
        var codes = RoleSeeds.Select(x => x.Code).ToArray();
        var existingRoles = await dbContext.Roles
            .Where(x => codes.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, cancellationToken);

        var hasChanges = false;

        foreach (var roleSeed in RoleSeeds)
        {
            if (!existingRoles.TryGetValue(roleSeed.Code, out var role))
            {
                dbContext.Roles.Add(new Role
                {
                    Code = roleSeed.Code,
                    Name = roleSeed.Name
                });
                hasChanges = true;
                continue;
            }

            if (!string.Equals(role.Name, roleSeed.Name, StringComparison.Ordinal))
            {
                role.Name = roleSeed.Name;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<User> SeedAdminUserAsync(
        ReportSystemDbContext dbContext,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return await UpsertUserAsync(
            dbContext,
            AdminUserId,
            AdminEmployeeCode,
            AdminFullName,
            AdminEmail,
            utcNow,
            cancellationToken);
    }

    private static async Task<User> SeedManagerUserAsync(
        ReportSystemDbContext dbContext,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return await UpsertUserAsync(
            dbContext,
            ManagerUserId,
            ManagerEmployeeCode,
            ManagerFullName,
            ManagerEmail,
            utcNow,
            cancellationToken);
    }

    private static async Task<User> SeedEmployeeUserAsync(
        ReportSystemDbContext dbContext,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return await UpsertUserAsync(
            dbContext,
            EmployeeUserId,
            EmployeeEmployeeCode,
            EmployeeFullName,
            EmployeeEmail,
            utcNow,
            cancellationToken);
    }

    private static async Task SeedAdminRoleAsync(
        ReportSystemDbContext dbContext,
        Guid adminUserId,
        CancellationToken cancellationToken)
    {
        await SeedUserRoleAsync(dbContext, adminUserId, AdminRoleCode, cancellationToken);
    }

    private static async Task SeedUserRoleAsync(
        ReportSystemDbContext dbContext,
        Guid userId,
        string roleCode,
        CancellationToken cancellationToken)
    {
        var roleId = await dbContext.Roles
            .Where(x => x.Code == roleCode)
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);

        var hasRoleMapping = await dbContext.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);

        if (hasRoleMapping)
        {
            return;
        }

        dbContext.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<User> UpsertUserAsync(
        ReportSystemDbContext dbContext,
        Guid userId,
        string employeeCode,
        string fullName,
        string email,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(x => x.EmployeeCode == employeeCode, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Id = userId,
                EmployeeCode = employeeCode,
                FullName = fullName,
                Email = email,
                IsActive = true,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        var hasChanges = false;

        if (!string.Equals(user.FullName, fullName, StringComparison.Ordinal))
        {
            user.FullName = fullName;
            hasChanges = true;
        }

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = email;
            hasChanges = true;
        }

        if (!user.IsActive)
        {
            user.IsActive = true;
            hasChanges = true;
        }

        if (hasChanges)
        {
            user.UpdatedAt = utcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return user;
    }

    private static async Task SeedTemplatesAsync(
        ReportSystemDbContext dbContext,
        Guid adminUserId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        foreach (var templateSeed in TemplateSeeds)
        {
            var template = await UpsertTemplateAsync(dbContext, templateSeed, utcNow, cancellationToken);
            var version = await UpsertPublishedVersionAsync(dbContext, template.Id, adminUserId, utcNow, cancellationToken);
            await UpsertFieldsAndRulesAsync(dbContext, version.Id, templateSeed.Fields, utcNow, cancellationToken);
        }
    }

    private static async Task<ReportTemplate> UpsertTemplateAsync(
        ReportSystemDbContext dbContext,
        TemplateSeed templateSeed,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var template = await dbContext.ReportTemplates
            .SingleOrDefaultAsync(x => x.TemplateCode == templateSeed.TemplateCode, cancellationToken);

        if (template is null)
        {
            template = new ReportTemplate
            {
                TemplateCode = templateSeed.TemplateCode,
                TemplateName = templateSeed.TemplateName,
                Description = templateSeed.Description,
                IsActive = true,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            dbContext.ReportTemplates.Add(template);
            await dbContext.SaveChangesAsync(cancellationToken);
            return template;
        }

        var hasChanges = false;

        if (!string.Equals(template.TemplateName, templateSeed.TemplateName, StringComparison.Ordinal))
        {
            template.TemplateName = templateSeed.TemplateName;
            hasChanges = true;
        }

        if (!string.Equals(template.Description, templateSeed.Description, StringComparison.Ordinal))
        {
            template.Description = templateSeed.Description;
            hasChanges = true;
        }

        if (!template.IsActive)
        {
            template.IsActive = true;
            hasChanges = true;
        }

        if (hasChanges)
        {
            template.UpdatedAt = utcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return template;
    }

    private static async Task<ReportTemplateVersion> UpsertPublishedVersionAsync(
        ReportSystemDbContext dbContext,
        long templateId,
        Guid adminUserId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        const int versionNo = 1;
        const string status = "PUBLISHED";

        var version = await dbContext.ReportTemplateVersions
            .SingleOrDefaultAsync(x => x.TemplateId == templateId && x.VersionNo == versionNo, cancellationToken);

        if (version is null)
        {
            version = new ReportTemplateVersion
            {
                TemplateId = templateId,
                VersionNo = versionNo,
                Status = status,
                EffectiveFrom = utcNow,
                EffectiveTo = null,
                PublishedBy = adminUserId,
                PublishedAt = utcNow,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            dbContext.ReportTemplateVersions.Add(version);
            await dbContext.SaveChangesAsync(cancellationToken);
            return version;
        }

        var hasChanges = false;

        if (!string.Equals(version.Status, status, StringComparison.Ordinal))
        {
            version.Status = status;
            hasChanges = true;
        }

        if (version.EffectiveFrom is null)
        {
            version.EffectiveFrom = utcNow;
            hasChanges = true;
        }

        if (version.PublishedBy != adminUserId)
        {
            version.PublishedBy = adminUserId;
            hasChanges = true;
        }

        if (version.PublishedAt is null)
        {
            version.PublishedAt = utcNow;
            hasChanges = true;
        }

        if (hasChanges)
        {
            version.UpdatedAt = utcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return version;
    }

    private static async Task UpsertFieldsAndRulesAsync(
        ReportSystemDbContext dbContext,
        long templateVersionId,
        IReadOnlyCollection<FieldSeed> fieldSeeds,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var existingFields = await dbContext.TemplateFields
            .Where(x => x.TemplateVersionId == templateVersionId)
            .ToDictionaryAsync(x => x.FieldCode, cancellationToken);

        var hasFieldChanges = false;

        foreach (var fieldSeed in fieldSeeds)
        {
            if (!existingFields.TryGetValue(fieldSeed.FieldCode, out var field))
            {
                field = new TemplateField
                {
                    TemplateVersionId = templateVersionId,
                    FieldCode = fieldSeed.FieldCode,
                    FieldLabel = fieldSeed.FieldLabel,
                    DataType = fieldSeed.DataType,
                    Unit = fieldSeed.Unit,
                    IsRequired = fieldSeed.IsRequired,
                    DisplayOrder = fieldSeed.DisplayOrder,
                    Placeholder = fieldSeed.Placeholder,
                    OptionsJson = fieldSeed.OptionsJson,
                    IsActive = true,
                    CreatedAt = utcNow,
                    UpdatedAt = utcNow
                };

                dbContext.TemplateFields.Add(field);
                existingFields[fieldSeed.FieldCode] = field;
                hasFieldChanges = true;
                continue;
            }

            var fieldChanged = false;

            if (!string.Equals(field.FieldLabel, fieldSeed.FieldLabel, StringComparison.Ordinal))
            {
                field.FieldLabel = fieldSeed.FieldLabel;
                fieldChanged = true;
            }

            if (!string.Equals(field.DataType, fieldSeed.DataType, StringComparison.Ordinal))
            {
                field.DataType = fieldSeed.DataType;
                fieldChanged = true;
            }

            if (!string.Equals(field.Unit, fieldSeed.Unit, StringComparison.Ordinal))
            {
                field.Unit = fieldSeed.Unit;
                fieldChanged = true;
            }

            if (field.IsRequired != fieldSeed.IsRequired)
            {
                field.IsRequired = fieldSeed.IsRequired;
                fieldChanged = true;
            }

            if (field.DisplayOrder != fieldSeed.DisplayOrder)
            {
                field.DisplayOrder = fieldSeed.DisplayOrder;
                fieldChanged = true;
            }

            if (!string.Equals(field.Placeholder, fieldSeed.Placeholder, StringComparison.Ordinal))
            {
                field.Placeholder = fieldSeed.Placeholder;
                fieldChanged = true;
            }

            if (!string.Equals(field.OptionsJson, fieldSeed.OptionsJson, StringComparison.Ordinal))
            {
                field.OptionsJson = fieldSeed.OptionsJson;
                fieldChanged = true;
            }

            if (!field.IsActive)
            {
                field.IsActive = true;
                fieldChanged = true;
            }

            if (fieldChanged)
            {
                field.UpdatedAt = utcNow;
                hasFieldChanges = true;
            }
        }

        if (hasFieldChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var fieldSeed in fieldSeeds.Where(x => x.Rules.Count > 0))
        {
            if (!existingFields.TryGetValue(fieldSeed.FieldCode, out var field))
            {
                continue;
            }

            var existingRules = await dbContext.FieldRules
                .Where(x => x.FieldId == field.Id)
                .ToDictionaryAsync(x => x.RuleOrder, cancellationToken);

            var hasRuleChanges = false;

            foreach (var ruleSeed in fieldSeed.Rules)
            {
                if (!existingRules.TryGetValue(ruleSeed.RuleOrder, out var rule))
                {
                    dbContext.FieldRules.Add(new FieldRule
                    {
                        FieldId = field.Id,
                        RuleOrder = ruleSeed.RuleOrder,
                        RuleType = ruleSeed.RuleType,
                        MinValue = ruleSeed.MinValue,
                        MaxValue = ruleSeed.MaxValue,
                        ThresholdValue = ruleSeed.ThresholdValue,
                        ExpectedText = ruleSeed.ExpectedText,
                        Severity = ruleSeed.Severity,
                        FailMessage = ruleSeed.FailMessage,
                        IsActive = true,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow
                    });
                    hasRuleChanges = true;
                    continue;
                }

                var ruleChanged = false;

                if (!string.Equals(rule.RuleType, ruleSeed.RuleType, StringComparison.Ordinal))
                {
                    rule.RuleType = ruleSeed.RuleType;
                    ruleChanged = true;
                }

                if (rule.MinValue != ruleSeed.MinValue)
                {
                    rule.MinValue = ruleSeed.MinValue;
                    ruleChanged = true;
                }

                if (rule.MaxValue != ruleSeed.MaxValue)
                {
                    rule.MaxValue = ruleSeed.MaxValue;
                    ruleChanged = true;
                }

                if (rule.ThresholdValue != ruleSeed.ThresholdValue)
                {
                    rule.ThresholdValue = ruleSeed.ThresholdValue;
                    ruleChanged = true;
                }

                if (!string.Equals(rule.ExpectedText, ruleSeed.ExpectedText, StringComparison.Ordinal))
                {
                    rule.ExpectedText = ruleSeed.ExpectedText;
                    ruleChanged = true;
                }

                if (!string.Equals(rule.Severity, ruleSeed.Severity, StringComparison.Ordinal))
                {
                    rule.Severity = ruleSeed.Severity;
                    ruleChanged = true;
                }

                if (!string.Equals(rule.FailMessage, ruleSeed.FailMessage, StringComparison.Ordinal))
                {
                    rule.FailMessage = ruleSeed.FailMessage;
                    ruleChanged = true;
                }

                if (!rule.IsActive)
                {
                    rule.IsActive = true;
                    ruleChanged = true;
                }

                if (ruleChanged)
                {
                    rule.UpdatedAt = utcNow;
                    hasRuleChanges = true;
                }
            }

            if (hasRuleChanges)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private sealed record RoleSeed(string Code, string Name);

    private sealed record TemplateSeed(
        string TemplateCode,
        string TemplateName,
        string? Description,
        IReadOnlyCollection<FieldSeed> Fields);

    private sealed record FieldSeed(
        string FieldCode,
        string FieldLabel,
        string DataType,
        string? Unit,
        bool IsRequired,
        int DisplayOrder,
        string? Placeholder,
        string? OptionsJson,
        IReadOnlyCollection<RuleSeed> Rules);

    private sealed record RuleSeed(
        int RuleOrder,
        string RuleType,
        decimal? MinValue,
        decimal? MaxValue,
        decimal? ThresholdValue,
        string? ExpectedText,
        string Severity,
        string FailMessage);
}
