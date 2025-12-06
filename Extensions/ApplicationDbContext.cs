using Microsoft.EntityFrameworkCore;

namespace TelerikReportingRestService.Extensions
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
           : base(dbContextOptions)
        {
        }
    }
}