using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ResourceCurator
{
    [Serializable]
    public class CronResourceProducerSettings
    {
        public string CronSchedule { get; set; } = "* * * * * *";
    }

    public abstract class CronResourceProducer<TResource> : CronResourceProducer<TResource, CronResourceProducerSettings>, IResourceProducer<TResource>
        where TResource : IResource
    {
        protected CronResourceProducer(string name, CronResourceProducerSettings settings, ISerializer serializer) : base(name, settings, serializer) { }
        protected CronResourceProducer(string name, CronResourceProducerSettings settings, ISerializer serializer, IScheduler scheduler) : base(name, settings, serializer, scheduler) { }
    }
    // ToDo: (?) maybe better use only one class with ValueTask
    public abstract class CronResourceProducer<TResource, TSettings> : ResourceProducer<TSettings>, IResourceProducer<TResource>
         where TResource : IResource
         where TSettings : CronResourceProducerSettings, new()
    {
        /// <summary>
        /// Must be used as current time accessor
        /// </summary>
        protected readonly IScheduler _scheduler;

        protected CronResourceProducer(string name, TSettings settings, ISerializer serializer)
            : this(name, settings, serializer, Scheduler.Default) { }

        protected CronResourceProducer(string name, TSettings settings, ISerializer serializer, IScheduler scheduler)
            : base(name, settings, serializer)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            // We skip first run with `DateTimeOffset.MinValue`
            Resource = ObservableExtensions.CronSchedule(Settings.CronSchedule, _scheduler).Skip(1).Select(_ => PullResource()).Publish().RefCount();

        }

        public abstract TResource PullResource();

        public IObservable<TResource> Resource { get; }
    }


    public abstract class CronResourceProducerAsync<TResource> : CronResourceProducerAsync<TResource, CronResourceProducerSettings>, IResourceProducer<TResource>
        where TResource : IResource
    {
        protected CronResourceProducerAsync(string name, CronResourceProducerSettings settings, ISerializer serializer) : base(name, settings, serializer) { }
        protected CronResourceProducerAsync(string name, CronResourceProducerSettings settings, ISerializer serializer, IScheduler scheduler) : base(name, settings, serializer, scheduler) { }
    }

    public abstract class CronResourceProducerAsync<TResource, TSettings> : ResourceProducer<TSettings>, IResourceProducer<TResource>
         where TResource : IResource
         where TSettings : CronResourceProducerSettings, new()
    {
        /// <summary>
        /// Must be used as current time accessor
        /// </summary>
        protected readonly IScheduler _scheduler;

        protected CronResourceProducerAsync(string name, TSettings settings, ISerializer serializer)
            : this(name, settings, serializer, Scheduler.Default) { }

        protected CronResourceProducerAsync(string name, TSettings settings, ISerializer serializer, IScheduler scheduler)
            : base(name, settings, serializer)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            // We skip first run with `DateTimeOffset.MinValue`
            Resource = ObservableExtensions
                .CronSchedule(Settings.CronSchedule, _scheduler)
                .Skip(1)
                // we can use Select().Concat() or SelectMany for async logic
                // ToDo: add overrides for concurrent execution N resources (via SelectMany)
                // More info:
                // https://github.com/dotnet/reactive/issues/459#issuecomment-357648243
                // https://stackoverflow.com/questions/23006852/howto-call-back-async-function-from-rx-subscribe
                .Select(_ => Observable.FromAsync<TResource>(async () => await PullResourceAsync().ConfigureAwait(false)))
                .Concat()
                .Publish()
                .RefCount();

        }

        public abstract Task<TResource> PullResourceAsync();

        public IObservable<TResource> Resource { get; }
    }

}