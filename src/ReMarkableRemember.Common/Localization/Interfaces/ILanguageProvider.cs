using System;
using System.Collections.Generic;

namespace ReMarkableRemember.Common.Localization.Interfaces;

public interface ILanguageProvider
{
    ILocalStrings Current { get; }

    String CurrentCode { get; }

    String DefaultCode { get; }

    IEnumerable<String> SupportedCodes { get; }

    void Switch(String code);
}
