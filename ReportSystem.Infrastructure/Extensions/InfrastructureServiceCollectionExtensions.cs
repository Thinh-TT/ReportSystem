using Microsoft.Extensions.DependencyInjection;
using ReportSystem.Application.Services.Workflow;
using ReportSystem.Infrastructure.Services;

namespace ReportSystem.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ISubmissionWorkflowService, SubmissionWorkflowService>();
        return services;
    }
}
