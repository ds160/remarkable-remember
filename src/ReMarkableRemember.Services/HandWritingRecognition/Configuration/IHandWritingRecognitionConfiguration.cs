using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.HandWritingRecognition.Configuration;

public interface IHandWritingRecognitionConfiguration : IConfiguration
{
    String Language { get; set; }
}