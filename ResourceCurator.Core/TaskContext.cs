using System;
using System.Collections.Generic;

namespace ResourceCurator
{
    public interface ITaskContext
    {
        IServiceProvider Services { get; }
        IDictionary<object, object> Items { get; }
    }

    internal class TaskContext : ITaskContext
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<object, object> _items;
        public TaskContext(IServiceProvider services)
        {
            _services = services;
            _items = new Dictionary<object, object>();
        }

        public IServiceProvider Services => _services;
        public IDictionary<object, object> Items => _items;
    }
}