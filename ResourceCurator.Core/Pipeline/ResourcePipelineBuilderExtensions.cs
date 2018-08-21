using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceCurator
{

    public static class ResourcePipelineBuilderExtensions
    {
        public static IResourcePipelineBuilder<TResource> Task<TResource>(this IResourcePipelineBuilder<TResource> builder, Action action)
            where TResource: IResource
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.Task(next => { action(); return ctx => next(ctx); });
        }
        public static IResourcePipelineBuilder<TResource> Task<TResource>(this IResourcePipelineBuilder<TResource> builder, Type pipelineTaskType)
            where TResource: IResource
        {
            if (!typeof(IPipelineTask<TResource>).GetTypeInfo().IsAssignableFrom(pipelineTaskType))
                throw new ArgumentException($"Type must implement {nameof(IPipelineTask<TResource>)}", nameof(pipelineTaskType));

            if (!(builder is ResourcePipelineBuilder<TResource> pipelineBuilder))
                throw new ArgumentException($"{nameof(builder)} must be {nameof(ResourcePipelineBuilder<TResource>)} instance", nameof(builder));

            var task = (IPipelineTask<TResource>) ActivatorUtilities.CreateInstance(pipelineBuilder._scope.ServiceProvider, pipelineTaskType);

            if (task is IDisposable disposable)
                pipelineBuilder._disposables.Add(disposable);

            return pipelineBuilder.Task(next => async context => {
                await task.InvokeAsync(context, next).ConfigureAwait(false);
            });
        }

        public static IResourcePipelineBuilder<TResource> Task<TResource, TPipelineTask>(this IResourcePipelineBuilder<TResource> builder) 
            where TResource: IResource
            where TPipelineTask: IPipelineTask<TResource>
            => builder.Task(typeof(TPipelineTask));


    }

}
