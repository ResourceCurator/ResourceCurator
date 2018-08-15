using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceCurator
{
    public interface ICuratorPipelineBuilder
    {
        /// <summary>
        /// Setup schedule via cron format string
        /// </summary>
        /// <param name="cronSchedule">"* * * * * *" format, with seconds</param>
        /// <returns></returns>
        ICuratorPipelineBuilder WithSchedule(string cronSchedule);
        ICuratorPipelineBuilder Task(Func<TaskDelegate, TaskDelegate> func);
    }

    public sealed class CuratorPipelineBuilder : ICuratorPipelineBuilder
    {
        private readonly CuratorPipeline _pipeline;
        internal readonly IServiceProvider _serviceProvider;

        internal CuratorPipelineBuilder(CuratorPipeline pipeline, IServiceProvider serviceProvider)
        {
            _pipeline = pipeline;
            _serviceProvider = serviceProvider;
        }
        
        public ICuratorPipelineBuilder Task(Func<TaskDelegate, TaskDelegate> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            _pipeline.AddTask(func);
            return this;
        }

        public ICuratorPipelineBuilder WithSchedule(string cronSchedule)
        {
            if (string.IsNullOrWhiteSpace(cronSchedule))
                throw new ArgumentException("Cron schedule can't be empty", nameof(cronSchedule));

            _pipeline.SetSchedule(cronSchedule);
            return this;
        }
    }

    public static class CuratorPipelineBuilderExtensions
    {
        public static ICuratorPipelineBuilder Task(this ICuratorPipelineBuilder builder, Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return builder.Task(next => { action(); return ctx => next(ctx); });
        }
        public static ICuratorPipelineBuilder Task(this ICuratorPipelineBuilder builder, Type middlewareTaskType)
        {
            if (!typeof(IMiddlewareTask).GetTypeInfo().IsAssignableFrom(middlewareTaskType))
                throw new ArgumentException($"Type must implement {nameof(IMiddlewareTask)}", nameof(middlewareTaskType));

            if (!(builder is CuratorPipelineBuilder curatorPipelineBuilder))
                throw new ArgumentException($"{nameof(builder)} must be {nameof(CuratorPipelineBuilder)} instance", nameof(builder));

            var task = (IMiddlewareTask) ActivatorUtilities.CreateInstance(curatorPipelineBuilder._serviceProvider, middlewareTaskType);

            return curatorPipelineBuilder.Task(next => async context => {
                await task.InvokeAsync(context, next).ConfigureAwait(false);
            });
        }

        public static ICuratorPipelineBuilder Task<T>(this ICuratorPipelineBuilder builder) where T: class, IMiddlewareTask => builder.Task(typeof(T));

    }

}
