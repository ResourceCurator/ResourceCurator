using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceCurator
{

    public interface ICuratorConfigurationBuilder
    {
        ICuratorConfigurationBuilder SetDateTimeAccessor(Func<DateTimeOffset> datetimeAccessor);
        ICuratorConfigurationBuilder UseServiceProvider(IServiceProvider serviceProvider);
        ICuratorPipelineBuilder AddPipeline(string name);
        ICuratorConfiguration Build();
        
    }

    public interface ICuratorConfiguration
    {
        Func<DateTimeOffset> DatetimeAccessor { get; }
        IEnumerable<ICuratorPipeline> Pipelines { get; }
        IServiceProvider ServiceProvider { get; }
    }



    public sealed class CuratorConfigurationBuilder : ICuratorConfigurationBuilder
    {
        private CuratorConfiguration _config = new CuratorConfiguration();

        public ICuratorPipelineBuilder AddPipeline(string name)
        {
            if (_config.ServiceProvider == null)
                throw new InvalidOperationException($"Setup service provider isn't setted. Call '{nameof(UseServiceProvider)}' first");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Name of pipeline can't be empty or whitespace");

            var pipeline = new CuratorPipeline(name);
            _config.AddPipeline(pipeline);
            return new CuratorPipelineBuilder(pipeline, _config.ServiceProvider);
        }

        public ICuratorConfiguration Build() => _config;
        

        public ICuratorConfigurationBuilder SetDateTimeAccessor(Func<DateTimeOffset> datetimeAccessor)
        {
            _config.DatetimeAccessor = datetimeAccessor ?? throw new ArgumentNullException(nameof(datetimeAccessor));
            return this;
        }

        public ICuratorConfigurationBuilder UseServiceProvider(IServiceProvider serviceProvider)
        {
            _config.ServiceProvider = serviceProvider;
            return this;
        }
    }

    public sealed class CuratorConfiguration : ICuratorConfiguration
    {
        private List<ICuratorPipeline> _pipelines;
        internal void AddPipeline(ICuratorPipeline pipeline) => _pipelines.Add(pipeline);

        internal CuratorConfiguration()
        {
            DatetimeAccessor = () => DateTimeOffset.UtcNow;
            _pipelines = new List<ICuratorPipeline>();

        }

        public IServiceProvider ServiceProvider { get; internal set; }
        public Func<DateTimeOffset> DatetimeAccessor { get; internal set; }
        public IEnumerable<ICuratorPipeline> Pipelines => _pipelines.Select(x => x);
    }
}
