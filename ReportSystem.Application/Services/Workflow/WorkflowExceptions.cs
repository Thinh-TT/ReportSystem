namespace ReportSystem.Application.Services.Workflow;

public sealed class WorkflowNotFoundException : Exception
{
    public WorkflowNotFoundException(string message) : base(message)
    {
    }
}

public sealed class WorkflowRuleViolationException : Exception
{
    public WorkflowRuleViolationException(string message) : base(message)
    {
    }
}
