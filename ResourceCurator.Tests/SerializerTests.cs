using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ResourceCurator.Tests
{
    public class SerializerTests
    {
        private readonly ISerializer _yaml = new YamlSerializer();

        private const string _hulk =  "Name: Bruce\r\n"+
                                      "Nicknames:\r\n"+
                                      "- Green hero\r\n"+
                                      "- Hulk\r\n"+
                                      "Properties:\r\n"+
                                      "  Color: Green\r\n";
        [Fact]
        public void YamlSerializer_Serialize()
        {
            var toSerialize = new Human(){
                Name = "Bruce",
                Nicknames = new []{"Green hero", "Hulk"},
                Properties = new Dictionary<string, string>(){ { "Color", "Green" } },
            };
            var serialized = _yaml.SerializeToString(toSerialize);

            Assert.NotNull(serialized);
            Assert.Equal(_hulk, serialized);
        }

        [Fact]
        public void YamlSerializer_Deserialize()
        {
            var deserialized = _yaml.Deserialize<Human>(_hulk);

            Assert.NotNull(deserialized?.Properties);
            Assert.Equal("Bruce", deserialized.Name);
            Assert.Equal(2, deserialized.Nicknames.Count());
            Assert.Equal("Green", deserialized.Properties["Color"]);
        }

        public class Human
        {
            public string Name { get; set; }
            public IEnumerable<string> Nicknames { get; set; }
            public Dictionary<string, string> Properties { get; set; }
        }
    }
}
