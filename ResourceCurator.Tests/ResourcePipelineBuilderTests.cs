using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceCurator.Tests
{    
    public class ResourcePipelineBuilderTests
    {

        [Fact]
        public void ctor()
        {
            //TODO
            var pipeline = new ResourcePipeline<FakeIResource>("");
            var service = A.Fake<IServiceProvider>();
            var observable = A.Fake<IObservable<FakeIResource>>();

            //var pipelineBuilder = new ResourcePipelineBuilder<FakeIResource>(pipeline, service, observable);
          
        }

    }
}
