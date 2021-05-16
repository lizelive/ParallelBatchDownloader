using Microsoft.EntityFrameworkCore;

namespace ParallelBatchDownloader
{
    class DownloadContext : DbContext
    {
        public DbSet<Download> Downloads { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)=> options.UseSqlite("Data Source=D://download.db");
    }
}
