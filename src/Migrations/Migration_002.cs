using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Migrations;

[DbContext(typeof(DatabaseContext))]
[Migration("002 - Templates")]
public sealed class Migration002 : Migration
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null) { throw new ArgumentNullException(nameof(modelBuilder)); }

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

        modelBuilder.Entity("ReMarkableRemember.Entities.Template", builder =>
        {
            builder.Property<String>("Category").HasColumnType("TEXT");
            builder.Property<String>("Name").HasColumnType("TEXT");
            builder.Property<Byte[]>("BytesPng").IsRequired().HasColumnType("BLOB");
            builder.Property<Byte[]>("BytesSvg").IsRequired().HasColumnType("BLOB");
            builder.Property<String>("IconCode").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Category", "Name");
            builder.ToTable("Templates");
        });
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder == null) { throw new ArgumentNullException(nameof(migrationBuilder)); }

        migrationBuilder.CreateTable(
            name: "Templates",
            columns: table => new
            {
                Category = table.Column<String>(type: "TEXT", nullable: false),
                Name = table.Column<String>(type: "TEXT", nullable: false),
                IconCode = table.Column<String>(type: "TEXT", nullable: false),
                BytesPng = table.Column<Byte[]>(type: "BLOB", nullable: false),
                BytesSvg = table.Column<Byte[]>(type: "BLOB", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Templates", entity => new { entity.Category, entity.Name });
            }
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder == null) { throw new ArgumentNullException(nameof(migrationBuilder)); }

        migrationBuilder.DropTable("Templates");
    }
}
