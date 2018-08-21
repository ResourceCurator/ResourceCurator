using Cronos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceCurator
{

    /// <summary>
    /// Represent task that can be runned in pipeline
    /// </summary>
    /// <remarks>Created by DI container once and used every run of pipeline</remarks>
    public interface IPipelineTask<TResource>
        where TResource : IResource
    {
        Task InvokeAsync(IPipelineContext<TResource> context, PipelineTaskDelegate<TResource> nextPipelineTask);
    }

}