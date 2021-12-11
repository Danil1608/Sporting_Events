using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sporting_Events.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<CompetitionType> CompetitionTypes { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<AppFile> Files { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"Server={Environment.MachineName};Database=SportingEventsDb;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Request>()
                .HasOne(x => x.Account)
                .WithMany(x => x.Requests)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .HasOne(x => x.Account)
                .WithMany(x => x.Results)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasMany(x => x.Competitions)
                .WithMany(x => x.Accounts);

            modelBuilder.Entity<Competition>()
                .HasOne(x => x.Account)
                .WithMany(x => x.OrganizersCompetitions)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "admin" },
                new Role { Id = 2, Name = "sportsman" },
                new Role { Id = 3, Name = "organizer" }
            );

            modelBuilder.Entity<CompetitionType>().HasData(
                new CompetitionType { Id = 1, Name = "running" },
                new CompetitionType { Id = 2, Name = "longjumping" },
                new CompetitionType { Id = 3, Name = "rod" }
            );

            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = 1,
                Login = "admin",
                Password = Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("admin"))),
                Name = "Админ",
                MiddleName = "Админович",
                LastName = "Админов",
                Age = 21,
                Phone = "88005553535",
                RoleId = 1
            });
        }
    }
}
