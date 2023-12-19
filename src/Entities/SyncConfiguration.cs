using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

public class SyncConfiguration
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
