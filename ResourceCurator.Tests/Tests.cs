using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ResourceCurator.Tests
{
    public class Tests
    {

        [Fact]
        public static void Build_Default()
        {
            var config = new CuratorConfigurationBuilder().Build();
            var datetime = config?.DatetimeAccessor();

            Assert.NotNull(config?.DatetimeAccessor);
            Assert.NotNull(datetime);
        }

        [Fact]
        public static void AddPipeline()
        {
            var builder = new CuratorConfigurationBuilder().UseServiceProvider(A.Fake<IServiceProvider>());

            builder.AddPipeline("pipeline_1").WithSchedule("* * * * * *")
                                         .Task(() =>{ })
                                         .Task(() =>{ });

            builder.AddPipeline("pipeline_2").WithSchedule("* * * * * *")
                                         .Task(() =>{ })
                                         .Task(() =>{ });
            var config = builder.Build();

            Assert.NotNull(config);
            Assert.Equal(2, config.Pipelines.Count());
            Assert.Equal("pipeline_1", config.Pipelines.First().Name);
            Assert.Equal(2, ((CuratorPipeline)config.Pipelines.Last())._tasks.Count);
        }

        [Fact]
        public static void AddPipeline_WithoutServiceProvider_ThrowsException()
        {
            var builder = new CuratorConfigurationBuilder();

            Assert.Throws<ArgumentNullException>(() =>
            {
                builder.AddPipeline("pipeline_1").WithSchedule("* * * * * *").Task(() => { });
            });
       
        }


        [Fact]
        public async Task Curator_StartAsync()
        {
            var serviceProvider = A.Fake<IServiceProvider>();
            var taskRunCount = 0;
            var taskParams = new SimpleTaskParameters { Action = () => ++taskRunCount };
            A.CallTo(() => serviceProvider.GetService(A<Type>.That.IsEqualTo(typeof(ISimpleTaskParameters))))
             .Returns(taskParams);

            var builder = new CuratorConfigurationBuilder().UseServiceProvider(serviceProvider);

            var time = A.Fake<Func<DateTimeOffset>>();
            var timeSnapshot = DateTimeOffset.Now;
            A.CallTo(() => time()).ReturnsNextFromSequence(
                timeSnapshot,
                timeSnapshot.AddSeconds(1)
                );
            builder.SetDateTimeAccessor(time);
            var finishTaskRunCount = 0;
            builder.AddPipeline("_").Task(typeof(SimpleTask)).Task(typeof(SimpleTask)).Task(() => ++finishTaskRunCount);
            var config = builder.Build();

            // act

            using (var overseer = new Curator(config))
            {
                var cts = new CancellationTokenSource();
                _ = overseer.StartAsync(cts.Token);
                for (int i = 0; i < 20; ++i)
                {
                    if (taskRunCount >= 2)
                        break;
                    await Task.Delay(50);
                }
                cts.Cancel();
            }

            // assert
            Assert.Equal(2, taskRunCount);
            Assert.Equal(1, finishTaskRunCount);
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public interface ISimpleTaskParameters
        {
            Action Action { get; }
        }

        public class SimpleTaskParameters : ISimpleTaskParameters
        {
            public Action Action { get; set; }
            public int RunCount { get; set; }
        }

        public class SimpleTask : IMiddlewareTask
        {
            private readonly ISimpleTaskParameters _param;
            public SimpleTask(ISimpleTaskParameters param) => _param = param;

            public Task InvokeAsync(ITaskContext context, TaskDelegate next)
            {
                _param.Action();
                return next(context);
            }
        }
#pragma warning restore CA1034 // Nested types should not be visible
    }
}
