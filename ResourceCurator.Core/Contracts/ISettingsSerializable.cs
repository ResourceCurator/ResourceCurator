using System;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

namespace ResourceCurator
{

    /// <summary>
    /// Marks object that has settings with serializable type <typeparamref name="TSettings"/>
    /// </summary>
    /// <typeparam name="TSettings">Must be serializable</typeparam>
    public interface ISettingsSerializable<TSettings> where TSettings : new()
    {
        TSettings Settings { get; }
    }

    public interface ISettingsSerializableToString
    {
        string SerializedSettings { get; }
    }

    public interface ISettingsSerializableToString<TSettings> : ISettingsSerializableToString, ISettingsSerializable<TSettings>
        where TSettings : new()
    { }
}