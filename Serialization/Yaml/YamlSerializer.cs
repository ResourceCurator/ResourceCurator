using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using ResourceCurator;

namespace ResourceCurator.Serialization.Yaml
{
    public class YamlSerializer : ISerializer
    {
        // (?) YamlDotNet serializer/deserializer is thread-safe
        private readonly static Serializer _serializer = new Serializer();
        private readonly static Deserializer _desirializer = new Deserializer();
        private readonly static Encoding _encoding = Encoding.UTF8;
        public object Deserialize(Type toType, byte[] serialized)
        {
            if (toType == null)
                throw new ArgumentNullException(nameof(toType));

            if (serialized == null)
                throw new ArgumentNullException(nameof(serialized));

            if (serialized.Length <= 0)
                return null;

            return _desirializer.Deserialize(_encoding.GetString(serialized), toType);
        }

        public byte[] Serialize(object toSerialize)
        {
            return toSerialize is null ? null : _encoding.GetBytes(_serializer.Serialize(toSerialize));
        }
    }
}
