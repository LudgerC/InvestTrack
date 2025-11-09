using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using InvestTrack.Model.Data;

namespace InvestTrack.Model.Factories
{
    public class InvestTrackDbContextFactory : IDesignTimeDbContextFactory<InvestTrackDbContext>
    {
        public InvestTrackDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var cs = config.GetConnectionString("DefaultConnection")
                     ?? "Data Source=investtrack.db";

            var optionsBuilder = new DbContextOptionsBuilder<InvestTrackDbContext>();
            optionsBuilder.UseSqlite(cs);

            return new InvestTrackDbContext(optionsBuilder.Options);
        }
    }
}
