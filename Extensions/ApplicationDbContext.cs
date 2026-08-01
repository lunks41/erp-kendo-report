using Microsoft.EntityFrameworkCore;

namespace erpkendoreport.Extensions
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
           : base(dbContextOptions)
        {
        }
    }
}