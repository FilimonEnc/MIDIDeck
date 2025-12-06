using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MIDIDeck.Models;

namespace MIDIDeck.Data;

public class AppDbContext : DbContext
{
    public DbSet<KeyBinding> KeyBindings => Set<KeyBinding>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    private readonly string _dbPath;

    public AppDbContext()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "MIDIDeck");
        Directory.CreateDirectory(appFolder);
        _dbPath = Path.Combine(appFolder, "midideck.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyBinding>(entity =>
        {
            entity.HasIndex(e => e.MidiNoteNumber);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ActionValue).HasMaxLength(500);
            entity.Property(e => e.LaunchArguments).HasMaxLength(500);
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(1000);
        });
    }
}

