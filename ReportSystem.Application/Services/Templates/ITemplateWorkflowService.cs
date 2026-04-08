namespace ReportSystem.Application.Services.Templates;

public interface ITemplateWorkflowService
{
    Task<TemplateWorkflowTemplateResult> CreateTemplateAsync(
        CreateTemplateWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateWorkflowTemplateResult> UpdateTemplateAsync(
        long templateId,
        UpdateTemplateWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateWorkflowVersionResult> CreateVersionAsync(
        CreateTemplateVersionWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateWorkflowVersionResult> UpdateVersionAsync(
        long templateVersionId,
        UpdateTemplateVersionWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateWorkflowFieldResult> CreateFieldAsync(
        CreateTemplateFieldWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateWorkflowFieldResult> UpdateFieldAsync(
        long fieldId,
        UpdateTemplateFieldWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateWorkflowRuleResult> CreateRuleAsync(
        CreateFieldRuleWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateWorkflowRuleResult> UpdateRuleAsync(
        long ruleId,
        UpdateFieldRuleWorkflowRequest request,
        CancellationToken cancellationToken = default);
}
