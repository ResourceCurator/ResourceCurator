using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using Utils;
using System.Reactive.Concurrency;

namespace ResourceCurator
{

    public static partial class ServiceCollectionExtensions
    {
        #region AddScheduler
        public static IServiceCollection AddScheduler(this IServiceCollection services) => AddScheduler(services, Scheduler.Default);
        public static IServiceCollection AddScheduler(this IServiceCollection services, IScheduler scheduler)
        {
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));

            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(scheduler);
            return services;
        }
        public static IServiceCollection AddScheduler(this IServiceCollection services, Func<IServiceProvider, IScheduler> schedulerFactory)
        {
            if (schedulerFactory == null)
                throw new ArgumentNullException(nameof(schedulerFactory));

            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(schedulerFactory);
            return services;
        }
        #endregion AddScheduler

        #region AddResourceProducer
        public static IServiceCollection AddResourceProducer<TResource, TResourceProducer>(this IServiceCollection services, string name)
            where TResource : IResource
            where TResourceProducer : IResourceProducer<TResource>
            => AddResourceProducer<TResource, TResourceProducer>(services, name, null);


        public static IServiceCollection AddResourceProducer<TResource, TResourceProducer>(this IServiceCollection services, string name, object settings)
            where TResource : IResource
            where TResourceProducer : IResourceProducer<TResource>

        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!services.Any(s => s.ServiceType == typeof(IScheduler)))
                throw new InvalidOperationException($"Register {nameof(IScheduler)} first.");

            if (!services.Any(s => s.ServiceType == typeof(ISerializer)))
                throw new InvalidOperationException($"Register {nameof(ISerializer)} first.");

            // ToDo: Add more validators to settings

            services.AddSingleton<IResourceProducer<TResource>>(s => ActivatorUtilities.CreateInstance<TResourceProducer>(s,
                settings == null ? new object[] { name } : new object[] { name, settings }));
            services.AddSingleton<IResourceProducer>(s => s.GetServices<IResourceProducer<TResource>>().SingleOrDefault()
                ?? throw new ArgumentException($"{nameof(IResourceProducer)}<{typeof(TResource).Name}> {name} is registered twice", nameof(name)));

            var resourceGeneric = typeof(IResourceProducer<>).MakeGenericType(typeof(IResource<>).MakeGenericType(new[] { GetGenericResourceType(typeof(TResource)) }));

            services.AddSingleton(resourceGeneric, s => s.GetServices<IResourceProducer<TResource>>().SingleOrDefault()
                ?? throw new ArgumentException($"{nameof(IResourceProducer)}<{typeof(TResource).Name}> {name} is registered twice", nameof(name)));

            return services;

            Type GetGenericResourceType(Type type)
            {
                var generic = type
                    .GetInterfaces()
                    .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IResource<>));

                if (generic == null)
                    throw new ArgumentException($"Type '{type.AssemblyQualifiedName}' doesn't implement '{typeof(IResource<>).AssemblyQualifiedName}' \n" +
                                               $"(multiple implementation of {nameof(IResource)} not allowed)");

                return generic.GenericTypeArguments[0];
            }


        }
        #endregion AddResourceProducer
    }


}