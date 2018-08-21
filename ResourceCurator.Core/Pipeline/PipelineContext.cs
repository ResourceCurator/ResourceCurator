using System;
using System.Collections.Generic;

namespace ResourceCurator
{

    internal class PipelineContext<TResource> : IPipelineContext<TResource> 
        where TResource: IResource
    {
        private readonly Dictionary<object, object> _items;
        public PipelineContext(IServiceProvider services, TResource resourceValue)
        {
            _items = new Dictionary<object, object>();
            Services = services;
            Resource = resourceValue;
        }

        public IServiceProvider Services { get; }
        public IDictionary<object, object> Items => _items;
        public TResource Resource { get; }
    }
}