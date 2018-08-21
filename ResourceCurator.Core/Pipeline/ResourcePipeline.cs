using Cronos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceCurator
{
    internal sealed class ResourcePipeline<TResource> : IResourcePipeline<TResource>
        where TResource : IResource
    {
        internal IList<Func<PipelineTaskDelegate<TResource>, PipelineTaskDelegate<TResource>>> _tasks;
        public string Name { get; internal set; }

        public IResourcePipeline<TResource> AddTask(Func<PipelineTaskDelegate<TResource>, PipelineTaskDelegate<TResource>> middlewareTask)
        {
            _tasks.Add(middlewareTask);
            return this;
        }

        public PipelineTaskDelegate<TResource> Build()
        {
            PipelineTaskDelegate<TResource> lastDelegate = ctx => Task.CompletedTask;
            foreach (var task in _tasks.Reverse())
            {
                lastDelegate = task(lastDelegate);
            }
            return lastDelegate;
        }

        internal ResourcePipeline(string name)
        {
            Name = name;
            _tasks = new List<Func<PipelineTaskDelegate<TResource>, PipelineTaskDelegate<TResource>>>();
        }
    }
}