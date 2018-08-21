using System;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;
using YamlDotNet.Serialization;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace ResourceCurator
{
    [Serializable]
    public class CronResourceProducerSettings
    {
        public string CronSchedule { get; set; } = "* * * * * *";
    }

    public abstract class CronResourceProducer<TResource> : ResourceProducer<CronResourceProducerSettings>, IResourceProducer<TResource>
         where TResource : IResource
    {
        /// <summary>
        /// Must be used as current time accessor
        /// </summary>
        protected readonly IScheduler _scheduler;

        protected CronResourceProducer(string name, CronResourceProducerSettings settings, ISerializer serializer) 
            : this(name, settings, serializer, Scheduler.Default) { }

        protected CronResourceProducer(string name, CronResourceProducerSettings settings, ISerializer serializer, IScheduler scheduler)
            : base(name, settings, serializer)
        {
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));

            _scheduler = scheduler;

            // We skip first run with `DateTimeOffset.MinValue`
            Resource = ObservableExtensions.CronSchedule(Settings.CronSchedule, _scheduler).Skip(1).Select(x => PullResource()).Publish().RefCount();
            
        }

        public abstract TResource PullResource();

        public IObservable<TResource> Resource { get; }
    }

}