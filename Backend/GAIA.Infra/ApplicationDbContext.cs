using GAIA.Domain.Assessment.Entities;
using Microsoft.EntityFrameworkCore;

namespace GAIA.Infra;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Assessment> Assessments { get; set; }
}
