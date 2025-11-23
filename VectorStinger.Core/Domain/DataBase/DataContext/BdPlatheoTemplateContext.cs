using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VectorStinger.Core.Domain.DataBase.Models;

namespace VectorStinger.Core.Domain.DataBase.DataContext;

public partial class BdPlatheoTemplateContext : DbContext
{
    public BdPlatheoTemplateContext()
    {
    }

    public BdPlatheoTemplateContext(DbContextOptions<BdPlatheoTemplateContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EditableSection> EditableSections { get; set; }

    public virtual DbSet<FakeDomain> FakeDomains { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Template> Templates { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EditableSection>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PK__Editable__80EF0872D6FFFD02");

            entity.HasIndex(e => e.PageId, "idx_pageid");

            entity.Property(e => e.ContentSection).HasMaxLength(1000);
            entity.Property(e => e.SectionType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Page).WithMany(p => p.EditableSections)
                .HasForeignKey(d => d.PageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EditableS__PageI__08B54D69");
        });

        modelBuilder.Entity<FakeDomain>(entity =>
        {
            entity.HasKey(e => e.DomainId).HasName("PK__Domains__2498D75064F1755A");

            entity.HasIndex(e => e.DomainName, "UQ__Domains__64C17FF0259FED63").IsUnique();

            entity.HasIndex(e => e.UserId, "idx_userid");

            entity.Property(e => e.ConfiguredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DomainName).HasMaxLength(255);

            entity.HasOne(d => d.Template).WithMany(p => p.FakeDomains)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Domains_Templates");

            entity.HasOne(d => d.User).WithMany(p => p.FakeDomains)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Domains__UserId__5BE2A6F2");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.PageId).HasName("PK__Pages__C565B10442E4A080");

            entity.HasIndex(e => e.TemplateId, "idx_templateid");

            entity.Property(e => e.PageType).HasMaxLength(50);

            entity.HasOne(d => d.Template).WithMany(p => p.Pages)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pages__TemplateI__6383C8BA");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06FACCA130805");

            entity.HasIndex(e => e.QuestionType, "idx_questiontype");

            entity.Property(e => e.QuestionText).HasMaxLength(500);
            entity.Property(e => e.QuestionType).HasMaxLength(50);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Question__92C7A1FF6452B98A");

            entity.HasIndex(e => e.QuestionId, "idx_questionid");

            entity.Property(e => e.OptionText).HasMaxLength(255);

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuestionO__Quest__0A9D95DB");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(e => e.SessionId, "IX_Sessions");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Provider)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Token)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sessions_Users");
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Template__F87ADD27221EF539");

            entity.HasIndex(e => e.UserId, "idx_userid");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TemplateFileKey).HasMaxLength(500);
            entity.Property(e => e.TemplateUrl).HasMaxLength(700);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Templates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Templates__UserI__60A75C0F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C95717290");

            entity.HasIndex(e => e.OauthId, "UQ__Users__A6FBF2FBC45C4BA8").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534509E97B2").IsUnique();

            entity.HasIndex(e => e.OauthId, "idx_googleid");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.OauthId).HasMaxLength(100);
            entity.Property(e => e.OauthProvider).HasMaxLength(50);
            entity.Property(e => e.OauthToken).HasMaxLength(2000);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__UserAnsw__D482500471658269");

            entity.HasIndex(e => new { e.UserId, e.QuestionId }, "idx_userid_questionid");

            entity.Property(e => e.AnswerText).HasMaxLength(1000);
            entity.Property(e => e.AnsweredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Option).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK__UserAnswe__Optio__0C85DE4D");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAnswe__Quest__0D7A0286");

            entity.HasOne(d => d.User).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAnswe__UserI__5535A963");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
