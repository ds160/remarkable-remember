using System;
using System.Collections.Generic;

namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletTemplateIcon
{
    private static readonly List<TabletTemplateIcon> icons = new List<TabletTemplateIcon>()
    {
        new TabletTemplateIcon("\uE9FE", "Blank", false),
        new TabletTemplateIcon("\uE9FD", "Blank", true),
        new TabletTemplateIcon("\uE9AA", "Checklist double", true),
        new TabletTemplateIcon("\uE9AB", "Checklist", true),
        new TabletTemplateIcon("\uE9AC", "Dayplanner", true),
        new TabletTemplateIcon("\uE9AD", "Dots bottom", true),
        new TabletTemplateIcon("\uE9B4", "Dots top", true),
        new TabletTemplateIcon("\uE9B6", "Grid bottom", true),
        new TabletTemplateIcon("\uE9BC", "Grid margin large", true),
        new TabletTemplateIcon("\uE9C9", "Grid margin medium", true),
        new TabletTemplateIcon("\uE9B7", "Grid top", true),
        new TabletTemplateIcon("\uE9BA", "Lines bottom", true),
        new TabletTemplateIcon("\uE9B8", "Lines medium", true),
        new TabletTemplateIcon("\uE9B9", "Lines small", true),
        new TabletTemplateIcon("\uE9BB", "Lines top", true),
        new TabletTemplateIcon("\uE9C8", "Margin medium", true),
        new TabletTemplateIcon("\uE9CA", "Margin small", true),
        new TabletTemplateIcon("\uE9CC", "One storyboard 1", true),
        new TabletTemplateIcon("\uE9CB", "One storyboard 2", true),
        new TabletTemplateIcon("\uE9D4", "One storyboard", false),
        new TabletTemplateIcon("\uE9D7", "Two storyboards", false),
        new TabletTemplateIcon("\uE9CD", "Two storyboards", true),
        new TabletTemplateIcon("\uE9B5", "Four storyboards", true),
        new TabletTemplateIcon("\uE997", "Four storyboards", false),
        new TabletTemplateIcon("\uE9CE", "Weekplanner US", true),
        new TabletTemplateIcon("\uE9CF", "Weekplanner", true),
        new TabletTemplateIcon("\uE98F", "Checklist", false),
        new TabletTemplateIcon("\uE9FF", "Cornell", false),
        new TabletTemplateIcon("\uE991", "Dayplanner", false),
        new TabletTemplateIcon("\uE993", "Dots S bottom", false),
        new TabletTemplateIcon("\uE996", "Dots S top", false),
        new TabletTemplateIcon("\uE995", "Dots S", false),
        new TabletTemplateIcon("\uE994", "Dots large", false),
        new TabletTemplateIcon("\uE9F9", "Dots S", true),
        new TabletTemplateIcon("\uE9F8", "Dots large", true),
        new TabletTemplateIcon("\uE999", "Grid bottom", false),
        new TabletTemplateIcon("\uE99A", "Grid large", false),
        new TabletTemplateIcon("\uE99D", "Grid medium", false),
        new TabletTemplateIcon("\uE99E", "Grid small", false),
        new TabletTemplateIcon("\uE9FC", "Grid large", true),
        new TabletTemplateIcon("\uE9FB", "Grid medium", true),
        new TabletTemplateIcon("\uE9FA", "Grid small", true),
        new TabletTemplateIcon("\uE99B", "Grid margin large", false),
        new TabletTemplateIcon("\uE99C", "Grid margin medium", false),
        new TabletTemplateIcon("\uE99F", "Grid top", false),
        new TabletTemplateIcon("\uE9A5", "Lined bottom", false),
        new TabletTemplateIcon("\uE9A0", "Lined heading", false),
        new TabletTemplateIcon("\uE9A9", "Lined top", false),
        new TabletTemplateIcon("\uE9A6", "Lines large", false),
        new TabletTemplateIcon("\uE9A7", "Lines medium", false),
        new TabletTemplateIcon("\uE9A8", "Lines small", false),
        new TabletTemplateIcon("\uE9D0", "Margin large", false),
        new TabletTemplateIcon("\uE9D1", "Margin medium", false),
        new TabletTemplateIcon("\uE9D2", "Margin small", false),
        new TabletTemplateIcon("\uE9D8", "US College", false),
        new TabletTemplateIcon("\uE9D9", "US Legal", false),
        new TabletTemplateIcon("\uE9DC", "Weekplanner 1", false),
        new TabletTemplateIcon("\uE9DA", "Weekplanner 2", false),
        new TabletTemplateIcon("\uE9DB", "Weekplanner US", false),
        new TabletTemplateIcon("\uEA00", "Isometric", false),
        new TabletTemplateIcon("\uE9D5", "Perspective 1", false),
        new TabletTemplateIcon("\uE9D6", "Perspective 2", false),
        new TabletTemplateIcon("\uE990", "Calligraphy large", true),
        new TabletTemplateIcon("\uE9A1", "Calligraphy medium", true),
        new TabletTemplateIcon("\uE970", "Piano sheet large", true),
        new TabletTemplateIcon("\uE975", "Piano sheet medium", true),
        new TabletTemplateIcon("\uE976", "Piano sheet small", true),
        new TabletTemplateIcon("\uE98D", "Calligraphy large", false),
        new TabletTemplateIcon("\uE98E", "Calligraphy medium", false),
        new TabletTemplateIcon("\uE9D3", "Music", false),
        new TabletTemplateIcon("\uE9C0", "Music Bass tablature", false),
        new TabletTemplateIcon("\uE9B2", "Music Guitar chords", false),
        new TabletTemplateIcon("\uE9C5", "Music Guitar tablature", false),
        new TabletTemplateIcon("\uE977", "Piano sheet large", false),
        new TabletTemplateIcon("\uE978", "Piano sheet medium", false),
        new TabletTemplateIcon("\uE979", "Piano sheet small", false),
        new TabletTemplateIcon("\uE97B", "Hexagon large", false),
        new TabletTemplateIcon("\uE982", "Hexagon medium", false),
        new TabletTemplateIcon("\uE98C", "Hexagon small", false)
    };

    private TabletTemplateIcon(String code, String name, Boolean isLandscape)
    {
        this.Code = code;
        this.IsLandscape = isLandscape;
        this.Name = name;
    }

    public String Code { get; }
    public Boolean IsLandscape { get; }
    public String Name { get; }

    public static IEnumerable<TabletTemplateIcon> Icons { get { return icons; } }
}
