using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;

public interface IHandWritingRecognitionConfiguration : IConfiguration
{
    String Language { get; set; }
}
