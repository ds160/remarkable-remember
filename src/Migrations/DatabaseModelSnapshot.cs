using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Migrations;

[DbContext(typeof(DatabaseContext))]
public class DatabaseModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null) { throw new ArgumentNullException(nameof(modelBuilder)); }

        modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

        modelBuilder.Entity("ReMarkableRemember.Entities.Backup", builder =>
        {
            builder.Property<String>("Id").HasColumnType("TEXT");
            builder.Property<String>("Deleted").HasColumnType("TEXT");
            builder.Property<String>("Modified").IsRequired().HasColumnType("TEXT");
            builder.Property<String>("Name").IsRequired().HasColumnType("TEXT");
            builder.Property<String>("ParentCollectionId").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("Backups");
        });

        modelBuilder.Entity("ReMarkableRemember.Entities.Setting", builder =>
        {
            builder.Property<String>("Key").HasColumnType("TEXT");
            builder.Property<String>("Value").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Key");
            builder.ToTable("Settings");
        });

        modelBuilder.Entity("ReMarkableRemember.Entities.SyncConfiguration", builder =>
        {
            builder.Property<String>("Id").HasColumnType("TEXT");
            builder.Property<String>("TargetDirectory").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("SyncConfigurations");
        });

        modelBuilder.Entity("ReMarkableRemember.Entities.Sync", builder =>
        {
            builder.Property<String>("Id").HasColumnType("TEXT");
            builder.Property<String>("Downloaded").IsRequired().HasColumnType("TEXT");
            builder.Property<String>("Modified").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("Syncs");
        });
    }
}
