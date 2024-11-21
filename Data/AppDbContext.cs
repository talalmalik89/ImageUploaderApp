using Microsoft.EntityFrameworkCore;
using ImageUploaderApp.Models;

namespace ImageUploaderApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ImageData> ImageDatas { get; set; }

    }
}
