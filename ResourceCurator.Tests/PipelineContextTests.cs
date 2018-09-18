using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FakeItEasy;

namespace ResourceCurator.Tests
{
    public class PipelineContextTests
    {
        [Fact]
        public void Ctor()
        {
            var service = A.Fake<IServiceProvider>();
            var resource = A.Fake<PipelineContextFakeResource>();

            var pipelineContext = new PipelineContext<PipelineContextFakeResource>(service, resource);

            Assert.Equal(service, pipelineContext.Services);
            Assert.Equal(resource, pipelineContext.Resource);

        }        
    }

    public class PipelineContextFakeResource : IResource
    {
        public string Name { get; internal set; }
        public string ProducerHash { get; internal set; }
        public object UntypedValue { get; internal set; }
    }
}
