using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Preference> Preferences { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<CustomerPreference> CustomerPreferences { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);

                // Связь с Role
                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Employees)
                      .HasForeignKey(e => e.RoleId);
            });

            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).HasMaxLength(100);
                entity.Property(r => r.Description).HasMaxLength(500);
            });

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.FirstName).HasMaxLength(100);
                entity.Property(c => c.LastName).HasMaxLength(100);
                entity.Property(c => c.Email).HasMaxLength(255);

                // Связь с PromoCode
                entity.HasMany(c => c.PromoCodes)
                      .WithOne(p => p.Customer)
                      .HasForeignKey(p => p.CustomerId);

                // Связь с Preference через CustomerPreference
                entity.HasMany(c => c.CustomerPreferences)
                      .WithOne(cp => cp.Customer)
                      .HasForeignKey(cp => cp.CustomerId);
            });

            // Preference
            modelBuilder.Entity<Preference>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(100);

                // Связь с Customer через CustomerPreference
                entity.HasMany(p => p.CustomerPreferences)
                      .WithOne(cp => cp.Preference)
                      .HasForeignKey(cp => cp.PreferenceId);
            });

            // PromoCode
            modelBuilder.Entity<PromoCode>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Code).HasMaxLength(50);

                // Связь с Customer
                entity.HasOne(p => p.Customer)
                      .WithMany(c => c.PromoCodes)
                      .HasForeignKey(p => p.CustomerId);

                // Связь с Preference
                entity.HasOne(p => p.Preference)
                      .WithMany()
                      .HasForeignKey(p => p.PreferenceId);
            });

            // CustomerPreference (Many-to-Many)
            modelBuilder.Entity<CustomerPreference>(entity =>
            {
                entity.HasKey(cp => new { cp.CustomerId, cp.PreferenceId });

                entity.HasOne(cp => cp.Customer)
                      .WithMany(c => c.CustomerPreferences)
                      .HasForeignKey(cp => cp.CustomerId);

                entity.HasOne(cp => cp.Preference)
                      .WithMany(p => p.CustomerPreferences)
                      .HasForeignKey(cp => cp.PreferenceId);
            });
        }
    }
} 
