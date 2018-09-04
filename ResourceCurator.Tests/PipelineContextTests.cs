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
            var resource = A.Fake<FakeIResource>();

            var pipelineContext = new PipelineContext<FakeIResource>(service, resource);

            Assert.Equal(service, pipelineContext.Services);
            Assert.Equal(resource, pipelineContext.Resource);


        }        
    }
}
