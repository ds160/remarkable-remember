using ReMarkableRemember.Common.Localization.LocalStrings;

namespace ReMarkableRemember.Common.Localization;

public static class Language
{
    static Language()
    {
        Current = new Default();
    }

    public static ILocalStrings Current { get; }
}
