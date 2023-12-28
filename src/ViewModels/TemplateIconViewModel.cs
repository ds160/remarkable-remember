using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateIconViewModel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private TemplateIconViewModel(String key, String code)
    {
        this.Code = code;
        this.Image = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Templates/{key}.png")));

        if (key.StartsWith("LS ", StringComparison.OrdinalIgnoreCase))
        {
            this.Landscape = true;
            this.Name = key[3..];
        }
        else if (key.StartsWith("P ", StringComparison.OrdinalIgnoreCase))
        {
            this.Landscape = false;
            this.Name = key[2..];
        }
        else
        {
            throw new ArgumentException("Invalid icon key defined.", nameof(key));
        }
    }

    public String Code { get; }

    public Bitmap Image { get; }

    public Boolean Landscape { get; }

    public String Name { get; }

    internal static IEnumerable<TemplateIconViewModel> GetIcons()
    {
        List<TemplateIconViewModel> icons = new List<TemplateIconViewModel>()
        {
            new TemplateIconViewModel("P Blank", "\uE9FE"),
            new TemplateIconViewModel("LS Blank", "\uE9FD"),
            new TemplateIconViewModel("LS Checklist double", "\uE9AA"),
            new TemplateIconViewModel("LS Checklist", "\uE9AB"),
            new TemplateIconViewModel("LS Dayplanner", "\uE9AC"),
            new TemplateIconViewModel("LS Dots bottom", "\uE9AD"),
            new TemplateIconViewModel("LS Dots top", "\uE9B4"),
            new TemplateIconViewModel("LS Grid bottom", "\uE9B6"),
            new TemplateIconViewModel("LS Grid margin large", "\uE9BC"),
            new TemplateIconViewModel("LS Grid margin medium", "\uE9C9"),
            new TemplateIconViewModel("LS Grid top", "\uE9B7"),
            new TemplateIconViewModel("LS Lines bottom", "\uE9BA"),
            new TemplateIconViewModel("LS Lines medium", "\uE9B8"),
            new TemplateIconViewModel("LS Lines small", "\uE9B9"),
            new TemplateIconViewModel("LS Lines top", "\uE9BB"),
            new TemplateIconViewModel("LS Margin medium", "\uE9C8"),
            new TemplateIconViewModel("LS Margin small", "\uE9CA"),
            new TemplateIconViewModel("LS One storyboard 1", "\uE9CC"),
            new TemplateIconViewModel("LS One storyboard 2", "\uE9CB"),
            new TemplateIconViewModel("P One storyboard", "\uE9D4"),
            new TemplateIconViewModel("P Two storyboards", "\uE9D7"),
            new TemplateIconViewModel("LS Two storyboards", "\uE9CD"),
            new TemplateIconViewModel("LS Four storyboards", "\uE9B5"),
            new TemplateIconViewModel("P Four storyboards", "\uE997"),
            new TemplateIconViewModel("LS Weekplanner US", "\uE9CE"),
            new TemplateIconViewModel("LS Weekplanner", "\uE9CF"),
            new TemplateIconViewModel("P Checklist", "\uE98F"),
            new TemplateIconViewModel("P Cornell", "\uE9FF"),
            new TemplateIconViewModel("P Dayplanner", "\uE991"),
            new TemplateIconViewModel("P Dots S bottom", "\uE993"),
            new TemplateIconViewModel("P Dots S top", "\uE996"),
            new TemplateIconViewModel("P Dots S", "\uE995"),
            new TemplateIconViewModel("P Dots large", "\uE994"),
            new TemplateIconViewModel("LS Dots S", "\uE9F9"),
            new TemplateIconViewModel("LS Dots large", "\uE9F8"),
            new TemplateIconViewModel("P Grid bottom", "\uE999"),
            new TemplateIconViewModel("P Grid large", "\uE99A"),
            new TemplateIconViewModel("P Grid medium", "\uE99D"),
            new TemplateIconViewModel("P Grid small", "\uE99E"),
            new TemplateIconViewModel("LS Grid large", "\uE9FC"),
            new TemplateIconViewModel("LS Grid medium", "\uE9FB"),
            new TemplateIconViewModel("LS Grid small", "\uE9FA"),
            new TemplateIconViewModel("P Grid margin large", "\uE99B"),
            new TemplateIconViewModel("P Grid margin medium", "\uE99C"),
            new TemplateIconViewModel("P Grid top", "\uE99F"),
            new TemplateIconViewModel("P Lined bottom", "\uE9A5"),
            new TemplateIconViewModel("P Lined heading", "\uE9A0"),
            new TemplateIconViewModel("P Lined top", "\uE9A9"),
            new TemplateIconViewModel("P Lines large", "\uE9A6"),
            new TemplateIconViewModel("P Lines medium", "\uE9A7"),
            new TemplateIconViewModel("P Lines small", "\uE9A8"),
            new TemplateIconViewModel("P Margin large", "\uE9D0"),
            new TemplateIconViewModel("P Margin medium", "\uE9D1"),
            new TemplateIconViewModel("P Margin small", "\uE9D2"),
            new TemplateIconViewModel("P US College", "\uE9D8"),
            new TemplateIconViewModel("P US Legal", "\uE9D9"),
            new TemplateIconViewModel("P Weekplanner 1", "\uE9DC"),
            new TemplateIconViewModel("P Weekplanner 2", "\uE9DA"),
            new TemplateIconViewModel("P Weekplanner US", "\uE9DB"),
            new TemplateIconViewModel("P Isometric", "\uEA00"),
            new TemplateIconViewModel("P Perspective 1", "\uE9D5"),
            new TemplateIconViewModel("P Perspective 2", "\uE9D6"),
            new TemplateIconViewModel("LS Calligraphy large", "\uE990"),
            new TemplateIconViewModel("LS Calligraphy medium", "\uE9A1"),
            new TemplateIconViewModel("LS Piano sheet large", "\uE970"),
            new TemplateIconViewModel("LS Piano sheet medium", "\uE975"),
            new TemplateIconViewModel("LS Piano sheet small", "\uE976"),
            new TemplateIconViewModel("P Calligraphy large", "\uE98D"),
            new TemplateIconViewModel("P Calligraphy medium", "\uE98E"),
            new TemplateIconViewModel("P Music", "\uE9D3"),
            new TemplateIconViewModel("P Music Bass tablature", "\uE9C0"),
            new TemplateIconViewModel("P Music Guitar chords", "\uE9B2"),
            new TemplateIconViewModel("P Music Guitar tablature", "\uE9C5"),
            new TemplateIconViewModel("P Piano sheet large", "\uE977"),
            new TemplateIconViewModel("P Piano sheet medium", "\uE978"),
            new TemplateIconViewModel("P Piano sheet small", "\uE979"),
            new TemplateIconViewModel("P Hexagon large", "\uE97B"),
            new TemplateIconViewModel("P Hexagon medium", "\uE982"),
            new TemplateIconViewModel("P Hexagon small", "\uE98C"),
        };

        return icons.OrderBy(icon => icon.Landscape ? 1 : 0).ThenBy(icon => icon.Name);
    }
}
