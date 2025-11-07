using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InvestTrack.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace InvestTrack.Model.Factories
{
    public class InvestTrackDbContextFactory : IDesignTimeDbContextFactory<InvestTrackDbContext>
    {
        public InvestTrackDbContext CreateDbContext(string[] args)
        {
            // Laad de user secrets / config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<InvestTrackDbContextFactory>()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<InvestTrackDbContext>();
            optionsBuilder.UseSqlServer(
                config.GetConnectionString("DefaultConnection") ??
                "Server=(localdb)\\mssqllocaldb;Database=InvestTrackDB;Trusted_Connection=True;MultipleActiveResultSets=true");

            return new InvestTrackDbContext(optionsBuilder.Options);
        }
    }
}
