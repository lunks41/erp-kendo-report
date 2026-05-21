using Microsoft.EntityFrameworkCore;

namespace my_report.Extensions
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
           : base(dbContextOptions)
        {
        }
    }
}