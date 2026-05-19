using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.ConfigurationService.Tests.Fakes;

public sealed class TestConfiguration : ConfigurationBase
{
    public TestConfiguration() : base("TestPrefix")
    {
        this.StringValue = String.Empty;
        this.AnotherString = String.Empty;
    }

    public String StringValue { get; set; }

    public String AnotherString { get; set; }

    public Int32 IntegerValue { get; set; } // Non-string properties must be ignored by reflection scanner.

    public String ReadOnlyValue { get; } = "read-only"; // Must be skipped (no setter).
}
