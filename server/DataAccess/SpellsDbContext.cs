using System;
using System.Collections.Generic;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

// DbContext for hele domænet. Den binder EF Core til Postgres-tabellerne i schemaet "spells".
public partial class SpellsDbContext : DbContext
{
    // Constructor: modtager konfiguration (fx connection string) udefra via DI.
    public SpellsDbContext(DbContextOptions<SpellsDbContext> options)
        : base(options)
    {
    }

    // DbSet = "entry point" til tabellerne. EF bruger disse til queries og ændringer.
    public virtual DbSet<Class> Classes { get; set; } = null!;
    public virtual DbSet<School> Schools { get; set; } = null!;
    public virtual DbSet<Spell> Spells { get; set; } = null!;

    // Fluent API-konfiguration. Her sætter vi tabeller, nøgler, kolonnenavne, relationer m.m.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // -------- class --------
        modelBuilder.Entity<Class>(entity =>
        {
            // Primærnøgle for class
            entity.HasKey(e => e.Id).HasName("class_pkey");

            // Map til tabel: spells.class
            entity.ToTable("class", "spells");

            // Kolonne-mapping (holder navngivning i DB og C# i sync)
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");

            // CreatedAt default i DB (UTC now)
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
        });

        // -------- school --------
        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("school_pkey");
            entity.ToTable("school", "spells");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
        });

        // -------- spell (+ many-to-many til class via spellclassjunction) --------
        modelBuilder.Entity<Spell>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("spell_pkey");
            entity.ToTable("spell", "spells");

            // Simple felter
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
            entity.Property(e => e.Castingtime).HasColumnName("castingtime");
            entity.Property(e => e.Range).HasColumnName("range");
            entity.Property(e => e.Components).HasColumnName("components");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Higherlevel).HasColumnName("higherlevel");

            // Bool flags med default false i databasen
            entity.Property(e => e.Concentration)
                .HasDefaultValue(false)
                .HasColumnName("concentration");

            entity.Property(e => e.Ritual)
                .HasDefaultValue(false)
                .HasColumnName("ritual");

            // Oprettelsestidspunkt i DB
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");

            // FK -> school (valgfri). Sletning af school sætter FK i spell til NULL.
            entity.HasOne(d => d.School).WithMany(p => p.Spells)
                .HasForeignKey(d => d.Schoolid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("spell_schoolid_fkey");

            // many-to-many Spell <-> Class via et eksplicit join-table (spells.spellclassjunction)
            entity.HasMany(d => d.Classes).WithMany(p => p.Spells)
                .UsingEntity<Dictionary<string, object>>(
                    // Navnet på join-entity i modellen er kun internt; tabellen navngives nedenfor
                    "spellclassjunction",
                    // Right side: FK til Class
                    r => r.HasOne<Class>().WithMany()
                          .HasForeignKey("Classid")
                          .HasConstraintName("spellclassjunction_classid_fkey"),
                    // Left side: FK til Spell
                    l => l.HasOne<Spell>().WithMany()
                          .HasForeignKey("Spellid")
                          .HasConstraintName("spellclassjunction_spellid_fkey"),
                    // Join-konfiguration: composite PK, tabelnavn + kolonnenavne
                    j =>
                    {
                        j.HasKey("Spellid", "Classid").HasName("spellclassjunction_pkey");
                        j.ToTable("spellclassjunction", "spells");
                        j.IndexerProperty<string>("Spellid").HasColumnName("spellid");
                        j.IndexerProperty<string>("Classid").HasColumnName("classid");
                    });
        });

        // Hook til partial for yderligere konfiguration i andre filer (hvis ønsket)
        OnModelCreatingPartial(modelBuilder);
    }

    // Partial hook (kan implementeres i en anden fil for at holde mapping adskilt)
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}



