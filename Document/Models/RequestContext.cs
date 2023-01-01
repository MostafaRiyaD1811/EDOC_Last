using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Reflection.Metadata;

namespace Document.Models
{
    public class RequestContext : DbContext
    {
        public RequestContext()
        {

        }
        public RequestContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
           
            modelBuilder.Entity<Requester>()
                .HasOne(r => r.Manager)
                .WithMany(r => r.ManagedEmployees)
                .HasForeignKey(r => r.MgrCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            #region Enabled_Triggers
            #region Car Request
            modelBuilder.Entity<CarRequest>().ToTable(tb => tb.HasTrigger("Car_Trigger_INSERT_UPDATE"));
            modelBuilder.Entity<CarRequest>().ToTable(tb => tb.HasTrigger("Car_Trigger_Delete"));
            #endregion
            #region Job Plan
            modelBuilder.Entity<DomainAccount>().ToTable(tb => tb.HasTrigger("Domain_Trigger_INSERT_UPDATE"));
            modelBuilder.Entity<DomainAccount>().ToTable(tb => tb.HasTrigger("Domain_Trigger_Delete"));
            #endregion
            #region PR
            modelBuilder.Entity<JobPlanUpdate>().ToTable(tb => tb.HasTrigger("Job_Trigger_INSERT_UPDATE"));
            modelBuilder.Entity<JobPlanUpdate>().ToTable(tb => tb.HasTrigger("Job_Trigger_Delete"));
            #endregion
            #region Requester
            modelBuilder.Entity<PR>().ToTable(tb => tb.HasTrigger("PR_Trigger_INSERT_UPDATE"));
            modelBuilder.Entity<PR>().ToTable(tb => tb.HasTrigger("PR_Trigger_Delete"));
            #endregion
            #region Travel Disk
            modelBuilder.Entity<TravelDesk>().ToTable(tb => tb.HasTrigger("Travel_Trigger_INSERT_UPDATE"));
            modelBuilder.Entity<TravelDesk>().ToTable(tb => tb.HasTrigger("Travel_Trigger_Delete"));
            #endregion
            #region Voucher
            modelBuilder.Entity<Voucher>().ToTable(tb => tb.HasTrigger("Vouchers_Trigger_INSERT_UPDATE"));
            modelBuilder.Entity<Voucher>().ToTable(tb => tb.HasTrigger("Vouchers_Trigger_Delete"));
            #endregion
            #region Domain Account
            modelBuilder.Entity<Requester>().ToTable(tb => tb.HasTrigger("Requesters_Trigger_INSERT_UPDATE"));
            modelBuilder.Entity<Requester>().ToTable(tb => tb.HasTrigger("Requesters_Trigger_Delete"));
            #endregion

            #endregion
            #region Default Values
            modelBuilder.Entity<CarRequest_Audit>().Property(b => b.Id).HasDefaultValueSql("newid()");
            modelBuilder.Entity<DomainAccount_Audit>().Property(b => b.Id).HasDefaultValueSql("newid()");
            modelBuilder.Entity<JobPlanUpdate_Audit>().Property(b => b.Id).HasDefaultValueSql("newid()");
            modelBuilder.Entity<PR_Audit>().Property(b => b.Id).HasDefaultValueSql("newid()");
            modelBuilder.Entity<TravelDesk_Audit>().Property(b => b.Id).HasDefaultValueSql("newid()");
            modelBuilder.Entity<Voucher_Audit>().Property(b => b.Id).HasDefaultValueSql("newid()");
            modelBuilder.Entity<Requester_Audit>().Property(b => b.Id).HasDefaultValueSql("newid()");
            #endregion

        }

        public virtual DbSet<Requester> Requesters { get; set; }

        public virtual DbSet<CarRequest> CarRequests { get; set; }
        public virtual DbSet<Voucher> Vouchers { get; set; }
        public virtual DbSet<PR> PRs { get; set; }
        public virtual DbSet<JobPlanUpdate> JobPlanUpdates { get; set; }
        public virtual DbSet<DomainAccount> DomainAccounts { get; set; }
        public virtual DbSet<TravelDesk> TravelDesks { get; set; }
        public virtual DbSet<Requester_Audit> Requesters_Audit { get; set; }
        public virtual DbSet<CarRequest_Audit> CarRequests_Audit { get; set; }
        public virtual DbSet<Voucher_Audit> Vouchers_Audit { get; set; }
        public virtual DbSet<PR_Audit> PRs_Audit { get; set; }
        public virtual DbSet<JobPlanUpdate_Audit> JobPlanUpdates_Audit { get; set; }
        public virtual DbSet<DomainAccount_Audit> DomainAccounts_Audit { get; set; }
        public virtual DbSet<TravelDesk_Audit> TravelDesks_Audit { get; set; }




    }
}

