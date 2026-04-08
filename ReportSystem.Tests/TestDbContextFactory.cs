using Microsoft.EntityFrameworkCore;
using ReportSystem.Infrastructure.Data;

namespace ReportSystem.Tests;

internal static class TestDbContextFactory
{
    public static ReportSystemDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ReportSystemDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ReportSystemDbContext(options);
    }
}
