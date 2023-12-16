using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

public class SyncDocument
{
    public SyncDocument(String id, String modified, String downloaded)
    {
        this.Id = id;
        this.Modified = modified;
        this.Downloaded = downloaded;
    }

    [Key]
    public String Id { get; set; }

    [Required]
    public String Modified { get; set; }

    [Required]
    public String Downloaded { get; set; }
}
