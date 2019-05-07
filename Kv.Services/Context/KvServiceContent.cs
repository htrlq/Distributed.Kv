using KvServices.Context.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KvServices.Context
{
    public class KvServiceContent:DbContext
    {
        public DbSet<KvValidate> KvValidate { get; set; }
        public DbSet<Kv> Kv { get; set; }

        public KvServiceContent(DbContextOptions<KvServiceContent> options):base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KvValidate>(
                options =>
                {
                    options.HasKey(e => e.Id);
                    options
                        .HasOne(e => e.Kv)
                        .WithOne(d => d.KvValidate)
                        .HasForeignKey<KvValidate>(e => e.KvId);
                }
            );
            modelBuilder.Entity<Kv>(
                options =>
                {
                    options.HasKey(e => e.Id);
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
