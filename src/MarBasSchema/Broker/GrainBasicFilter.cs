﻿using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public class GrainBasicFilter : IGrainBasicFilter
    {
        public IEnumerable<ITypeConstraint>? TypeConstraints { get; set; }
        public IEnumerable<Guid>? IdConstraints { get; set; }
    }
}
