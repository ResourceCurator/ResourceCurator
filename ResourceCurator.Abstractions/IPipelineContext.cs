using System;
using System.Collections.Generic;

namespace ResourceCurator
{
    public interface IPipelineContext
    {
        IServiceProvider Services { get; }
        IDictionary<object, object> Items { get; }

    }
    public interface IPipelineContext<TResource> : IPipelineContext
        where TResource: IResource
    {
        TResource Resource { get; }
    }
}