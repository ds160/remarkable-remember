using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Migrations;

[DbContext(typeof(DatabaseContext))]
[Migration("001 - Initial Create")]
public class Migration001 : Migration
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null) { return; }

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
            builder.Property<String>("Destination").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("SyncConfigurations");
        });

        modelBuilder.Entity("ReMarkableRemember.Entities.SyncDocument", builder =>
        {
            builder.Property<String>("Id").HasColumnType("TEXT");
            builder.Property<String>("Downloaded").IsRequired().HasColumnType("TEXT");
            builder.Property<String>("Modified").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("SyncDocuments");
        });
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder == null) { return; }

        migrationBuilder.CreateTable(
            name: "Backups",
            columns: table => new
            {
                Id = table.Column<String>(type: "TEXT", nullable: false),
                Name = table.Column<String>(type: "TEXT", nullable: false),
                ParentCollectionId = table.Column<String>(type: "TEXT", nullable: false),
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
                Destination = table.Column<String>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SyncConfigurations", entity => entity.Id);
            }
        );

        migrationBuilder.CreateTable(
            name: "SyncDocuments",
            columns: table => new
            {
                Id = table.Column<String>(type: "TEXT", nullable: false),
                Modified = table.Column<String>(type: "TEXT", nullable: false),
                Downloaded = table.Column<String>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SyncDocuments", entity => entity.Id);
            }
        );

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder == null) { return; }

        migrationBuilder.DropTable("Backups");
        migrationBuilder.DropTable("Settings");
        migrationBuilder.DropTable("SyncConfigurations");
        migrationBuilder.DropTable("SyncDocuments");
    }
}
