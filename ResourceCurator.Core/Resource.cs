using System;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ResourceCurator
{
    public interface IResource
    {
        string Name { get; }
        object UntypedValue { get; }
    }

    public interface IResource<T> : IResource
    {
        T Value { get; }
    }

    public abstract class Resource<T> : IResource<T>, IEquatable<Resource<T>>
    {
        protected Resource(T value) => Value = value;

        public abstract string Name { get; }
        public T Value { get; protected set; }
        public object UntypedValue => Value;

        #region Equals + GetHashCode

        public override bool Equals(object obj) => Equals(obj as Resource<T>);

#pragma warning disable CA1062 // Validate arguments of public methods

        // other checked by `is null` it's not cause boxing (7.9.6 of the C# language spec) or call operator ==,
        public bool Equals(Resource<T> other) => !(other is null) && EqualityComparer<T>.Default.Equals(Value, other.Value);

#pragma warning restore CA1062 // Validate arguments of public methods

        public override int GetHashCode() => (Value, Name).GetHashCode();

        public static bool operator ==(Resource<T> resource1, Resource<T> resource2) => EqualityComparer<Resource<T>>.Default.Equals(resource1, resource2);

        public static bool operator !=(Resource<T> resource1, Resource<T> resource2) => !(resource1 == resource2);

        #endregion Equals + GetHashCode

        public override string ToString() => "Name " + Name ?? "null" + " Value: " + Value ?? "null";
        
    }
    public interface IResourceProducer
    {
        Task StartAsync(CancellationToken cancellationToken);
    }

    public interface IResourceProducer<T> : IResourceProducer where T: IResource
    {
        IObservable<T> Resource { get; }
    }
}