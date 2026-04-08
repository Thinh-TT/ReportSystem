using Microsoft.AspNetCore.Mvc;
using ReportSystem.Application.Services.Workflow;

namespace ReportSystem.Web.Controllers;

[ApiController]
[Route("api/submissions")]
public class SubmissionWorkflowController : ControllerBase
{
    private readonly ISubmissionWorkflowService _workflowService;

    public SubmissionWorkflowController(ISubmissionWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListSubmissionsApiRequest request, CancellationToken cancellationToken)
    {
        var query = new SubmissionListQueryRequest
        {
            TemplateVersionId = request.TemplateVersionId,
            ReportDateFrom = request.ReportDateFrom,
            ReportDateTo = request.ReportDateTo,
            Status = request.Status,
            CreatedByUserId = request.CreatedByUserId
        };

        try
        {
            var result = await _workflowService.ListAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (WorkflowRuleViolationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("draft")]
    public async Task<IActionResult> CreateDraft([FromBody] CreateDraftSubmissionRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(() => _workflowService.CreateDraftAsync(request, cancellationToken));
    }

    [HttpPut("{submissionId:long}/fields")]
    public async Task<IActionResult> UpdateFieldValues(
        [FromRoute] long submissionId,
        [FromBody] UpdateFieldValuesApiRequest request,
        CancellationToken cancellationToken)
    {
        var workflowRequest = new UpdateSubmissionFieldValuesRequest
        {
            SubmissionId = submissionId,
            FieldValues = request.FieldValues
        };

        return await ExecuteAsync(() => _workflowService.UpdateFieldValuesAsync(workflowRequest, cancellationToken));
    }

    [HttpPost("{submissionId:long}/submit")]
    public async Task<IActionResult> Submit(
        [FromRoute] long submissionId,
        [FromBody] SubmitApiRequest request,
        CancellationToken cancellationToken)
    {
        var workflowRequest = new SubmitSubmissionRequest
        {
            SubmissionId = submissionId,
            ActionByUserId = request.ActionByUserId
        };

        return await ExecuteAsync(() => _workflowService.SubmitAsync(workflowRequest, cancellationToken));
    }

    [HttpPost("{submissionId:long}/evaluate")]
    public async Task<IActionResult> AutoEvaluate([FromRoute] long submissionId, CancellationToken cancellationToken)
    {
        var request = new AutoEvaluateSubmissionRequest { SubmissionId = submissionId };
        return await ExecuteAsync(() => _workflowService.AutoEvaluateAsync(request, cancellationToken));
    }

    [HttpPost("{submissionId:long}/approve")]
    public async Task<IActionResult> Approve(
        [FromRoute] long submissionId,
        [FromBody] ApproveApiRequest request,
        CancellationToken cancellationToken)
    {
        var workflowRequest = new ApproveSubmissionRequest
        {
            SubmissionId = submissionId,
            ActionByUserId = request.ActionByUserId,
            ManagerResult = request.ManagerResult,
            ManagerNote = request.ManagerNote
        };

        return await ExecuteAsync(() => _workflowService.ApproveAsync(workflowRequest, cancellationToken));
    }

    [HttpPost("{submissionId:long}/reject")]
    public async Task<IActionResult> Reject(
        [FromRoute] long submissionId,
        [FromBody] RejectApiRequest request,
        CancellationToken cancellationToken)
    {
        var workflowRequest = new RejectSubmissionRequest
        {
            SubmissionId = submissionId,
            ActionByUserId = request.ActionByUserId,
            ManagerNote = request.ManagerNote
        };

        return await ExecuteAsync(() => _workflowService.RejectAsync(workflowRequest, cancellationToken));
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task<SubmissionWorkflowResult>> operation)
    {
        try
        {
            var result = await operation();
            return Ok(result);
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (WorkflowRuleViolationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    public sealed class UpdateFieldValuesApiRequest
    {
        public List<SubmissionFieldValueInput> FieldValues { get; set; } = [];
    }

    public sealed class ListSubmissionsApiRequest
    {
        public long? TemplateVersionId { get; set; }
        public DateOnly? ReportDateFrom { get; set; }
        public DateOnly? ReportDateTo { get; set; }
        public string? Status { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }

    public sealed class SubmitApiRequest
    {
        public Guid ActionByUserId { get; set; }
    }

    public sealed class ApproveApiRequest
    {
        public Guid ActionByUserId { get; set; }
        public string ManagerResult { get; set; } = "PASS";
        public string? ManagerNote { get; set; }
    }

    public sealed class RejectApiRequest
    {
        public Guid ActionByUserId { get; set; }
        public string? ManagerNote { get; set; }
    }
}
