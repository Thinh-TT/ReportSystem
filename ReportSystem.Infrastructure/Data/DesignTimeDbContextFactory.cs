using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportSystem.Infrastructure.Data
{
    public class ReportSystemDbContextFactory
    : IDesignTimeDbContextFactory<ReportSystemDbContext>
    {
        public ReportSystemDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ReportSystemDbContext>();

            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=ReportSystem;User Id=sa;Password=Thinh@12345;TrustServerCertificate=True");

            return new ReportSystemDbContext(optionsBuilder.Options);
        }
    }
}
