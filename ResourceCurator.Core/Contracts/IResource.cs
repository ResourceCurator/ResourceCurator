using System;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

namespace ResourceCurator
{
    public interface IResource
    {
        string Name { get; }
        /// <summary>
        /// Define who is create this resource
        /// </summary>
        string ProducerHash { get; }
        object UntypedValue { get; }
    }

    public interface IResource<T> : IResource
    {
        T Value { get; }
    }
}