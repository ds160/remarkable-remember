using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;

public sealed class HandWritingRecognitionConfigurationMyScript : ConfigurationBase, IHandWritingRecognitionConfiguration
{
    public HandWritingRecognitionConfigurationMyScript() : base("MyScript") { }

    public String ApplicationKey { get; set; } = String.Empty;

    public String HmacKey { get; set; } = String.Empty;

    public String Language { get; set; } = "en_US";
}
