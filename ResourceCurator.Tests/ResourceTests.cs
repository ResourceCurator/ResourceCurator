using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using Xunit;

namespace ResourceCurator.Tests
{
    public class ResourceTests
    {
        [Fact]
        public void Ctor()
        {
            var res1 = new FakeResource("", 0);
            var res2 = new FakeResource("", 0);
            var res3 = new FakeResource("", 1);

            Assert.True(res1.Equals(res2));
            Assert.True(res1.GetHashCode() == res2.GetHashCode());
            Assert.True(res1.GetHashCode() != res3.GetHashCode());
            Assert.True(res1 == res2);
            Assert.True(res1 != res3);
        }
        
        class FakeResource : Resource<int>
        {
            public override string Name => "";

            public FakeResource(string producerHash, int value) : base(producerHash, value)
            {
            }
        }
    }
}
