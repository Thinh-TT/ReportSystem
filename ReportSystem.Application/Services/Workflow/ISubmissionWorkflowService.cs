namespace ReportSystem.Application.Services.Workflow;

public interface ISubmissionWorkflowService
{
    Task<IReadOnlyCollection<SubmissionListItem>> ListAsync(SubmissionListQueryRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionWorkflowResult> CreateDraftAsync(CreateDraftSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionWorkflowResult> UpdateFieldValuesAsync(UpdateSubmissionFieldValuesRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionWorkflowResult> SubmitAsync(SubmitSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionWorkflowResult> AutoEvaluateAsync(AutoEvaluateSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionWorkflowResult> ApproveAsync(ApproveSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionWorkflowResult> RejectAsync(RejectSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionWorkflowResult> ReopenAsync(ReopenSubmissionRequest request, CancellationToken cancellationToken = default);
}
