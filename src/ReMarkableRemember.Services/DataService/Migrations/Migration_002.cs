using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReMarkableRemember.Services.DataService.Database;

namespace ReMarkableRemember.Services.DataService.Migrations;

[DbContext(typeof(DatabaseContext))]
[Migration("002 - Templates")]
public sealed class Migration002 : Migration
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

        modelBuilder.Entity("ReMarkableRemember.Services.DataService.Entities.Backup", builder =>
        {
            builder.Property<String>("Id").HasColumnType("TEXT");
            builder.Property<String>("Deleted").HasColumnType("TEXT");
            builder.Property<String>("Modified").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("Backups");
        });

        modelBuilder.Entity("ReMarkableRemember.Services.DataService.Entities.Setting", builder =>
        {
            builder.Property<String>("Key").HasColumnType("TEXT");
            builder.Property<String>("Value").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Key");
            builder.ToTable("Settings");
        });

        modelBuilder.Entity("ReMarkableRemember.Services.DataService.Entities.SyncConfiguration", builder =>
        {
            builder.Property<String>("Id").HasColumnType("TEXT");
            builder.Property<String>("TargetDirectory").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("SyncConfigurations");
        });

        modelBuilder.Entity("ReMarkableRemember.Services.DataService.Entities.Sync", builder =>
        {
            builder.Property<String>("Id").HasColumnType("TEXT");
            builder.Property<String>("Modified").IsRequired().HasColumnType("TEXT");
            builder.Property<String>("Path").IsRequired().HasColumnType("TEXT");
            builder.HasKey("Id");
            builder.ToTable("Syncs");
        });

        modelBuilder.Entity("ReMarkableRemember.Services.DataService.Entities.Template", builder =>
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
        ArgumentNullException.ThrowIfNull(migrationBuilder);

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
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable("Templates");
    }
}
