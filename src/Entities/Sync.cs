using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

public class Sync
{
    public Sync(String id, DateTime modified, String downloaded)
    {
        this.Id = id;
        this.Modified = modified;
        this.Downloaded = downloaded;
    }

    [Key]
    public String Id { get; set; }

    [Required]
    public DateTime Modified { get; set; }

    [Required]
    public String Downloaded { get; set; }
}
