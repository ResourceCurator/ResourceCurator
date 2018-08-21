using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using Utils;

namespace ResourceCurator
{
    /// <summary>
    /// We can't use directly ServiceCollection as is, because we need
    /// access producer by name, this accessor do it for us
    /// </summary>
    public interface IResourceProducerAccessor
    {
        IResourceProducer<TResource> GetProducer<TResource>(string producerName) where TResource : IResource;
    }

    public class ResourceProducerAccessor : IResourceProducerAccessor
    {
        private readonly Dictionary<(string Name, Type ResourceType), IResourceProducer> _producers;
        private static IEqualityComparer<(string Name, Type ResourceType)> _equalityComparer =
            new LambdaComparer<(string Name, Type ResourceType)>((x1, x2) => string.Equals(x1.Name, x2.Name, StringComparison.Ordinal) && x2.ResourceType == x1.ResourceType);

        public ResourceProducerAccessor(IEnumerable<IResourceProducer> resourceProducers)
        {
            if (resourceProducers == null)
                throw new ArgumentNullException(nameof(resourceProducers));

            _producers = resourceProducers.ToDictionary(
                kv => (kv.Name, GetProducerResourceType(kv)),
                kv => kv,
                // we want use ordinal comparer for string
                _equalityComparer
                );

            Type GetProducerResourceType(IResourceProducer obj)
            {
                if (obj == null)
                    throw new ArgumentNullException(nameof(resourceProducers), "Resource producer IEnumerable can't contains null");
                var type = obj.GetType();
                var generic = type
                    .GetInterfaces()
                    .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IResourceProducer<>));


                if (generic == null)
                    throw new ArgumentException($"Type '{type.AssemblyQualifiedName}' doesn't implement '{typeof(IResourceProducer<>).AssemblyQualifiedName}' \n" +
                                               $"(multiple implementation of {nameof(IResourceProducer)} not allowed)",nameof(resourceProducers));

                return generic.GenericTypeArguments[0];
            }

        }

        public IResourceProducer<TResource> GetProducer<TResource>(string producerName) where TResource : IResource
            => _producers.TryGetValue((producerName, typeof(TResource)), out IResourceProducer result)
                ? (IResourceProducer<TResource>)result
                : throw new KeyNotFoundException($"Resource producer with name '{producerName}' isn't registered. \n" +
                                                 $"Must register this producer before {nameof(IResourceProducerAccessor)}");
    }

}