using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Services.DataService.Entities;

internal sealed class Sync
{
    public Sync(String id, DateTime modified, String path)
    {
        this.Id = id;
        this.Modified = modified;
        this.Path = path;
    }

    [Key]
    public String Id { get; set; }

    [Required]
    public DateTime Modified { get; set; }

    [Required]
    public String Path { get; set; }
}
