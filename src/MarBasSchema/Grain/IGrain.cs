﻿using System.ComponentModel;
using MarBasCommon;

namespace MarBasSchema.Grain
{
    public interface IGrain: IIdentifiable, INamed, ITyped
    {
        Guid? ParentId { get; }
        [ReadOnly(true)]
        string? Path { get; }
        DateTime CTime { get; }
        DateTime MTime { get; }
        string Owner { get; }
        int Revision { get; set; }
        string? SortKey { get; set; }
        int CustomFlag { get; set; }
        string? XAttrs { get; set; }
    }
}
