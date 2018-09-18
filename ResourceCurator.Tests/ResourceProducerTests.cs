using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FakeItEasy;

namespace ResourceCurator.Tests
{
    public class ResourceProducerTests
    {
        [Fact]
        public void Ctor()
        {
            var name = "Test1";
            var resource1 = new FakeResourceProducer("Test1");
            var resource2 = new FakeResourceProducer("Test2");

            Assert.Equal(resource1.Name, name);
            Assert.NotEqual(resource1.Hash, resource2.Hash);
            
            Assert.Throws<ArgumentNullException>(()=> new FakeResourceProducer(null));
        }

        [Fact]
        public void HashBytes()
        {
            var resource1 = new FakeResourceProducer("Test1");
            var resource2 = new FakeResourceProducer("Test2");

            var a1 = resource1.HashBytes();
            var a2 = resource2.HashBytes();

            Assert.NotEqual(a1, a2);
        }

        private class FakeResourceProducer : ResourceProducer
        {
            public FakeResourceProducer(string name) : base(name)
            { }
        }
    }
}
