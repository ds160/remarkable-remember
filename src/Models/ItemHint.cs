using System;

namespace ReMarkableRemember.Models;

[Flags]
public enum ItemHint
{
    None = 0x00,
    NotFoundInTarget = 0x01,
    SyncPathChanged = 0x02,
    Modified = 0x04,
    New = 0x08,
    ExistsInTarget = 0x10
}
