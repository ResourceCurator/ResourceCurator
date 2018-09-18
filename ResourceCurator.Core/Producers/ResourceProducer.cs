using System;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;
using System.Diagnostics;
using System.ComponentModel;
using Utils;

namespace ResourceCurator
{
    /// <summary>
    /// Base class for all producers
    /// </summary>
    public abstract class ResourceProducer : IResourceProducer
    {
        public string Name { get; protected set; }

        private string _hash;
        public string Hash => _hash ?? (_hash = ComputeHash());

        /// <summary>
        /// This constructor must be public in inherited class
        /// </summary>
        /// <param name="name">Some self-description name of resource producer, must be unique in type</param>
        protected ResourceProducer(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        protected internal virtual string ComputeHash()
        {
            var encoding = Encoding.UTF8;
            using (var hashFunc = HashAlgorithm.Create("System.Security.Cryptography.SHA256"))
            {
                var sb = StringBuilderCache.Get(64);
                var hash = hashFunc.ComputeHash(HashBytes());
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToStringRecycle();
            }
        }

        [DebuggerStepThrough]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected internal virtual byte[] HashBytes() => Encoding.UTF8.GetBytes(GetType().FullName + Name);

        [DebuggerStepThrough]
        public override string ToString() => $"{Name} [{Hash}]";
    }


    public abstract class ResourceProducer<TSettings> : ResourceProducer, ISettingsSerializableToString<TSettings>
        where TSettings : new()
    {

        protected readonly ISerializer _serializer;
        protected readonly string _serializedSettings;
        /// <summary>
        /// Settings is readonly, any changes in object won't affect to behaviour
        /// </summary>
        public TSettings Settings { get; }
        public string SerializedSettings => _serializedSettings;

        protected ResourceProducer(string name, TSettings settings, ISerializer serializer) : base(name)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            Settings = settings;
            _serializer = serializer;
            _serializedSettings = _serializer.SerializeToString(settings);
        }

        protected internal override byte[] HashBytes() => Encoding.UTF8.GetBytes(GetType().FullName + Name + _serializedSettings);
    }

}