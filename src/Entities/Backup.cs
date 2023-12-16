using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

public class Backup
{
    public Backup(String id, String name, String parentCollectionId, String modified)
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
    public String Modified { get; set; }

    public String? Deleted { get; set; }
}
