using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Cronos;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceCurator
{
    public static class ObservableExtensions
    {
        #region CronSchedule

        private static readonly Func<DateTimeOffset?, bool> _cronCachedCronCondition = d => d.HasValue;
        private static readonly Func<DateTimeOffset?, short> _cronCachedResultSelector = _ => 0;
        private static readonly Func<DateTimeOffset?, DateTimeOffset> _cronCachedTimeSelector = d => d.Value;

        [DebuggerStepThrough]
        public static IObservable<short> CronSchedule(string cronSchedule) => CronSchedule(cronSchedule, Scheduler.Default);

        public static IObservable<short> CronSchedule(string cronSchedule, IScheduler scheduler)
        {
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));

            if (string.IsNullOrWhiteSpace(cronSchedule))
                throw new ArgumentException("Can't be null or empty, use format with seconds `* * * * * *`", nameof(cronSchedule));

            var schedule = CronExpression.Parse(cronSchedule, CronFormat.IncludeSeconds);
            return Observable.Generate<DateTimeOffset?, short>(
                DateTimeOffset.MinValue,
                _cronCachedCronCondition,
                _ => schedule.GetNextOccurrence(scheduler.Now, TimeZoneInfo.Utc, false),
                _cronCachedResultSelector,
                _cronCachedTimeSelector,
                scheduler).Publish().RefCount();
        }

        #endregion CronSchedule

        /// <summary>
        /// Add pipeline for processing <typeparamref name="TResource"/>
        /// Pipeline tasks and DI objects created in new scope.
        /// Scope will be closed when you call Dispose (returned by <seealso cref="IResourcePipelineBuilder{TResource}.Subscribe"/>
        /// </summary>
        /// <typeparam name="TResource">Type of processing resource</typeparam>
        /// <param name="observable">Observable resource from producer</param>
        /// <param name="name">Name of pipeline, must be unique</param>
        /// <param name="serviceProvider">service provider for DI (<see cref="IServiceScopeFactory"/> is required)</param>
        /// <returns></returns>
        public static IResourcePipelineBuilder<TResource> Pipeline<TResource>(this IObservable<TResource> observable, string name, IServiceProvider serviceProvider)
            where TResource : IResource
        {
            if (observable == null)
                throw new ArgumentNullException(nameof(observable));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return new ResourcePipelineBuilder<TResource>(new ResourcePipeline<TResource>(name), serviceProvider, observable);
        }
    }
}