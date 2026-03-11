using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;

public sealed class HandWritingRecognitionConfigurationMyScript : ConfigurationBase, IHandWritingRecognitionConfiguration
{
    public HandWritingRecognitionConfigurationMyScript() : base("MyScript")
    {
        this.ApplicationKey = String.Empty;
        this.HmacKey = String.Empty;
        this.Language = "en_US";
    }

    public String ApplicationKey { get; set; }

    public String HmacKey { get; set; }

    public String Language { get; set; }
}
