using System;

namespace ResourceCurator
{
    public interface IResourcePipelineBuilder<TResource>
        where TResource: IResource
    {
        IResourcePipelineBuilder<TResource> Task(Func<PipelineTaskDelegate<TResource>, PipelineTaskDelegate<TResource>> pipelineTask);

        IDisposable Subscribe();
    }

}
