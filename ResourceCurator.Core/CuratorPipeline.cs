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
    /// Represens workflow unit in <see cref="ICurator"/>
    /// </summary>
    public interface ICuratorPipeline
    {
        string Name { get; }

        /// <summary>
        /// Build and return chain of pipeline calls
        /// </summary>
        /// <returns></returns>
        TaskDelegate Build();

        /// <summary>
        /// Returns datetime of pipeline next run
        /// </summary>
        /// <param name="from">Exclusive date from</param>
        /// <returns>DateTime of next run or null</returns>
        DateTimeOffset? GetNextRun(DateTimeOffset from);

        ICuratorPipeline SetSchedule(string cronSchedule);

        /// <summary>
        /// Add <see cref="Func<TaskDelegate, TaskDelegate>"/> to pipeleine
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        ICuratorPipeline AddTask(Func<TaskDelegate, TaskDelegate> middleware);
    }

    /// <summary>
    /// Represent task that can be runned in pipeline
    /// </summary>
    /// <remarks>Created by DI container once and used every run of pipeline</remarks>
    public interface IMiddlewareTask
    {
        Task InvokeAsync(ITaskContext context, TaskDelegate nextMiddleware);
    }

    /// <summary>
    /// Named <see cref="Func{ITaskContext, Task}"/> for readability
    /// </summary>
    public delegate Task TaskDelegate(ITaskContext context);


    public sealed class CuratorPipeline : ICuratorPipeline
    {
        // for prevent checking null every time
        private static readonly CronExpression _defaultCronExpression = CronExpression.Parse("* * * * * *", CronFormat.IncludeSeconds);

        private CronExpression _schedule;
        internal IList<Func<TaskDelegate, TaskDelegate>> _tasks;
        public string Name { get; internal set; }

        public TaskDelegate Build()
        {
            TaskDelegate lastDelegate = ctx => Task.CompletedTask;
            foreach (var task in _tasks.Reverse())
            {
                lastDelegate = task(lastDelegate);
            }
            return lastDelegate;
        }

        public DateTimeOffset? GetNextRun(DateTimeOffset from) => _schedule.GetNextOccurrence(from, TimeZoneInfo.Local, inclusive: false);

        public ICuratorPipeline SetSchedule(string cronSchedule)
        {
            _schedule = CronExpression.Parse(cronSchedule, CronFormat.IncludeSeconds);
            return this;
        }

        public ICuratorPipeline AddTask(Func<TaskDelegate, TaskDelegate> middleware)
        {
            _tasks.Add(middleware);
            return this;
        }

        internal CuratorPipeline(string name)
        {
            Name = name;
            _tasks = new List<Func<TaskDelegate, TaskDelegate>>();

            _schedule = _defaultCronExpression;
        }
    }

}