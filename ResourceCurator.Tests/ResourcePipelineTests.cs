using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FakeItEasy;
using System.Linq;

namespace ResourceCurator.Tests
{
    public class ResourcePipelineTests
    {
        [Fact]
        public void AddTask()
        {
            var pipeline = new ResourcePipeline<Cpu>("");
            var fakeTask1 = A.Fake<Func<PipelineTaskDelegate<Cpu>, PipelineTaskDelegate<Cpu>>>();
            var fakeTask2 = A.Fake<Func<PipelineTaskDelegate<Cpu>, PipelineTaskDelegate<Cpu>>>();

            pipeline.AddTask(fakeTask1);
            pipeline.AddTask(fakeTask2);

            Assert.NotNull(pipeline._tasks);
            Assert.Equal(2, pipeline._tasks.Count);
            Assert.Equal(fakeTask1, pipeline._tasks.First());
        }


        [Fact]
        public void Build()
        {
            var pipeline = new ResourcePipeline<Cpu>("");
            var fakeTask1 = A.Fake<Func<PipelineTaskDelegate<Cpu>, PipelineTaskDelegate<Cpu>>>();
            var fakeTask2 = A.Fake<Func<PipelineTaskDelegate<Cpu>, PipelineTaskDelegate<Cpu>>>();

            var isFakeTest2LastCalledInPipeline = false;
            A.CallTo(() => fakeTask1(A<PipelineTaskDelegate<Cpu>>._)).ReturnsLazily(callInfo => {
                var arg = callInfo.Arguments[0] as PipelineTaskDelegate<Cpu>;
                if (!isFakeTest2LastCalledInPipeline)
                    throw new Exception("Second task isn't last on pipeline");
                return arg;

            });
            A.CallTo(() => fakeTask2(A<PipelineTaskDelegate<Cpu>>._)).ReturnsLazily(callInfo =>
            {
                var arg = callInfo.Arguments[0] as PipelineTaskDelegate<Cpu>;
                isFakeTest2LastCalledInPipeline = true;
                return arg;
            });

            pipeline.AddTask(fakeTask1);
            pipeline.AddTask(fakeTask2);

            var pipelineDelegate = pipeline.Build();
            Assert.NotNull(pipelineDelegate);
            pipelineDelegate(A.Fake<IPipelineContext<Cpu>>());


            Assert.True(isFakeTest2LastCalledInPipeline);
            A.CallTo(() => fakeTask1(A<PipelineTaskDelegate<Cpu>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeTask2(A<PipelineTaskDelegate<Cpu>>._)).MustHaveHappenedOnceExactly();
        }

        public class Cpu : IResource
        {
            public string Name { get; internal set; }
            public string ProducerHash { get; internal set; }
            public object UntypedValue { get; internal set; }
        }
    }
}
