using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Migrations;

[DbContext(typeof(DatabaseContext))]
[Migration("001 - Initial Create")]
public sealed class Migration001 : Migration
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

        modelBuilder.Entity("ReMarkableRemember.Entities.Backup", builder =>
        {
            builder.Property<String>("Id").HasColumnType("TEXT");
            builder.Property<String>("Deleted").HasColumnType("TEXT");
            builder.Property<String>("Modified").IsRequired().HasColumnType("TEXT");
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
            builder.Property<String>("Modified").IsRequired().HasColumnType("TEXT");
            builder.Property<String>("Path").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("Syncs");
        });
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.CreateTable(
            name: "Backups",
            columns: table => new
            {
                Id = table.Column<String>(type: "TEXT", nullable: false),
                Modified = table.Column<String>(type: "TEXT", nullable: false),
                Deleted = table.Column<String>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Backups", entity => entity.Id);
            }
        );

        migrationBuilder.CreateTable(
            name: "Settings",
            columns: table => new
            {
                Key = table.Column<String>(type: "TEXT", nullable: false),
                Value = table.Column<String>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Settings", entity => entity.Key);
            }
        );

        migrationBuilder.CreateTable(
            name: "SyncConfigurations",
            columns: table => new
            {
                Id = table.Column<String>(type: "TEXT", nullable: false),
                TargetDirectory = table.Column<String>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SyncConfigurations", entity => entity.Id);
            }
        );

        migrationBuilder.CreateTable(
            name: "Syncs",
            columns: table => new
            {
                Id = table.Column<String>(type: "TEXT", nullable: false),
                Modified = table.Column<String>(type: "TEXT", nullable: false),
                Path = table.Column<String>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Syncs", entity => entity.Id);
            }
        );

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable("Backups");
        migrationBuilder.DropTable("Settings");
        migrationBuilder.DropTable("SyncConfigurations");
        migrationBuilder.DropTable("Syncs");
    }
}
