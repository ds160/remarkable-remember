using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

public class SyncConfiguration
{
    public SyncConfiguration(String id, String destination)
    {
        this.Id = id;
        this.Destination = destination;
    }

    [Key]
    public String Id { get; set; }

    [Required]
    public String Destination { get; set; }
}
