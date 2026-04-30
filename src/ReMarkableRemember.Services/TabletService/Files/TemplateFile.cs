using System;
using System.Collections.Generic;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.Services.TabletService.Files;

internal struct TemplatesFile
{
    public List<Template> Templates { get; set; }

    public struct Template
    {
        public IEnumerable<String> Categories { get; set; }
        public String Filename { get; set; }
        public String IconCode { get; set; }
        public Boolean? Landscape { get; set; }
        public String Name { get; set; }

        public static Template Convert(TabletTemplate template)
        {
            return new Template()
            {
                Categories = new List<String>() { template.Category },
                Filename = template.FileName,
                IconCode = template.IconCode,
                Landscape = template.Landscape,
                Name = template.Name
            };
        }
    }
}
