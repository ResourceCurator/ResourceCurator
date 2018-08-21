using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ResourceCurator
{

    public interface ISerializer
    {
        object Deserialize(Type toType, byte[] serialized);
        byte[] Serialize(object toSerialize);
    }

    public static class SerializerExtensions
    {
        [DebuggerStepThrough]
        public static TResult Deserialize<TResult>(this ISerializer serializer, byte[] serialized)
            => (TResult)(serializer ?? throw new ArgumentNullException(nameof(serialized))).Deserialize(typeof(TResult), serialized);
        
        [DebuggerStepThrough]
        public static TResult Deserialize<TResult>(this ISerializer serializer, string serialized)
            => (TResult)(serializer ?? throw new ArgumentNullException(nameof(serialized))).Deserialize(typeof(TResult), Encoding.UTF8.GetBytes(serialized));

        [DebuggerStepThrough]
        public static string SerializeToString(this ISerializer serializer, object toSerializer)
            => Encoding.UTF8.GetString((serializer ?? throw new ArgumentNullException(nameof(toSerializer))).Serialize(toSerializer));
    }
}
