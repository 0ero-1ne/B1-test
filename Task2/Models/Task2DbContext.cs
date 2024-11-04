using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Task2.Models;

public partial class Task2DbContext : DbContext
{
    public Task2DbContext()
    {
    }

    public Task2DbContext(DbContextOptions<Task2DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillType> BillTypes { get; set; }

    public virtual DbSet<File> Files { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__banks__3213E83F5967CAE0");

            entity.ToTable("banks");

            entity.HasIndex(e => e.Name, "UQ__banks__72E12F1B5EAF2A0B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bills__3213E83F750AE330");

            entity.ToTable("bills");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BillType).HasColumnName("bill_type");
            entity.Property(e => e.BookNumber).HasColumnName("book_number");
            entity.Property(e => e.FileId).HasColumnName("file_id");
            entity.Property(e => e.InsaldoActive)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("insaldo_active");
            entity.Property(e => e.InsaldoPassive)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("insaldo_passive");
            entity.Property(e => e.OutsaldoActive)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("outsaldo_active");
            entity.Property(e => e.OutsaldoPassive)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("outsaldo_passive");
            entity.Property(e => e.TurnoversCredit)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("turnovers_credit");
            entity.Property(e => e.TurnoversDebit)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("turnovers_debit");

            entity.HasOne(d => d.BillTypeNavigation).WithMany(p => p.Bills)
                .HasForeignKey(d => d.BillType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__bills__bill_type__403A8C7D");

            entity.HasOne(d => d.File).WithMany(p => p.Bills)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__bills__file_id__412EB0B6");
        });

        modelBuilder.Entity<BillType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bill_typ__3213E83FA49A8C8E");

            entity.ToTable("bill_types");

            entity.HasIndex(e => e.Title, "UQ__bill_typ__E52A1BB32C279F85").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__files__3213E83F158C2BA0");

            entity.ToTable("files");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BankId).HasColumnName("bank_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Bank).WithMany(p => p.Files)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__files__bank_id__3D5E1FD2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
