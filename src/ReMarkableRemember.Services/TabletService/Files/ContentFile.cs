using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReMarkableRemember.Services.TabletService.Files;

internal struct ContentFile
{
    public PagesContainer? CPages { get; set; }
    public String FileType { get; set; }
    public Int32 FormatVersion { get; set; }
    public IEnumerable<String>? Pages { get; set; }

    public struct PagesContainer
    {
        public Collection<Page> Pages { get; set; }

        public struct Page
        {
            public Object? Deleted { get; set; }
            public String Id { get; set; }
        }
    }
}
