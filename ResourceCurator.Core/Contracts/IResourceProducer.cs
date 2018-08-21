using System;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

namespace ResourceCurator
{

    public interface IResourceProducer
    {
        /// <summary>
        /// Unique name of <see cref="IResourceProducer"/>
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Hash of tuple (this.GetType().Name, Name, Settings(if has) )
        /// Must be unique
        /// Prefer use <see cref="System.Security.Cryptography.SHA256"/>
        /// <c>HashAlgorithm.Create("System.Security.Cryptography.SHA256")</c>
        /// </summary>
        string Hash { get; }

    }

    public interface IResourceProducer<TResource> : IResourceProducer where TResource : IResource
    {
        IObservable<TResource> Resource { get; }
    }


}