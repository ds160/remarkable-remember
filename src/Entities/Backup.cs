using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

public class Backup
{
    public Backup(String id, String name, String parentCollectionId, DateTime modified)
    {
        this.Id = id;
        this.Name = name;
        this.ParentCollectionId = parentCollectionId;
        this.Modified = modified;
    }

    [Key]
    public String Id { get; set; }

    [Required]
    public String Name { get; set; }

    [Required]
    public String ParentCollectionId { get; set; }

    [Required]
    public DateTime Modified { get; set; }

    public DateTime? Deleted { get; set; }
}
