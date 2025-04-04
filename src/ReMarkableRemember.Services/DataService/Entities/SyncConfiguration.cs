using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Services.DataService.Entities;

internal sealed class SyncConfiguration
{
    public SyncConfiguration(String id, String targetDirectory)
    {
        this.Id = id;
        this.TargetDirectory = targetDirectory;
    }

    [Key]
    public String Id { get; set; }

    [Required]
    public String TargetDirectory { get; set; }
}
