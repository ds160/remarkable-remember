using System;

namespace ReMarkableRemember.Models;

[Flags]
public enum ItemHint
{
    None = 0,
    NotFoundInTarget = 1,
    SyncPathChanged = 2,
    Modified = 4,
    New = 8,
    ExistsInTarget = 16
}
