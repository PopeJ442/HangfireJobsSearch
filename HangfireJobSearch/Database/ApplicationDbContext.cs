using HangfireJobSearch.Models;
using Microsoft.EntityFrameworkCore;

namespace HangfireJobSearch.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<CrawledJob> Staging_Jobs { get; set; }
        public DbSet<CrawledJobId> Staging_JobsId { get; set; }


  }
    
}




