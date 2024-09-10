﻿using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MarBasCommon
{
    public interface ILocalized : ILocalizable
    {
        [JsonIgnore]
        [IgnoreDataMember]
        new CultureInfo CultureInfo { get; }
        new string Culture { get; }
    }
}
