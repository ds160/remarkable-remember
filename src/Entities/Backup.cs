using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

public class Backup
{
    public Backup(String id, DateTime modified)
    {
        this.Id = id;
        this.Modified = modified;
    }

    [Key]
    public String Id { get; set; }

    [Required]
    public DateTime Modified { get; set; }

    public DateTime? Deleted { get; set; }
}
