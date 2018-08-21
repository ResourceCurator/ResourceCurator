using Cronos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceCurator
{
    public interface IResourcePipeline
    {
        string Name { get; }
    }

    /// <summary>
    /// Named <see cref="Func{ITaskContext{}, Task}"/> for readability
    /// </summary>
    public delegate Task PipelineTaskDelegate<TResource>(IPipelineContext<TResource> context)
        where TResource : IResource;


    internal interface IResourcePipeline<TResource> : IResourcePipeline
       where TResource : IResource
    {
        /// <summary>
        /// Add <see cref="Func<TaskDelegate, TaskDelegate>"/> to pipeleine
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        IResourcePipeline<TResource> AddTask(Func<PipelineTaskDelegate<TResource>, PipelineTaskDelegate<TResource>> middleware);

        /// <summary>
        /// Build and return chain of pipeline calls
        /// </summary>
        /// <returns></returns>
        PipelineTaskDelegate<TResource> Build();
    }

}