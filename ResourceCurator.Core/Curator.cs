using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Utils;

namespace ResourceCurator
{
    public interface ICurator : IDisposable
    {
        Task StartAsync(CancellationToken? cancellationToken = null);
    }

    public sealed class Curator : ICurator
    {
        private readonly ICuratorConfiguration _configuration;
        private CancellationTokenSource _stopCancellationTokenSource;

        public void Dispose()
        {
            if (_stopCancellationTokenSource == null)
                return;

            if (!_stopCancellationTokenSource.IsCancellationRequested)
                _stopCancellationTokenSource.Cancel();

            _stopCancellationTokenSource.Dispose();
            _stopCancellationTokenSource = null;
        }

        public Task StartAsync(CancellationToken? cancellationToken)
        {
            _stopCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

            return Task.Factory.StartNew(() =>
                {
                    var prevRun = _configuration.DatetimeAccessor();
                    var pipelines = _configuration.Pipelines.AsList();
                    var tuples = new (DateTimeOffset? NextRun, ICuratorPipeline Pipeline)[pipelines.Count];
                    var tupleComparer = Comparer<(DateTimeOffset? NextRun, ICuratorPipeline Pipeline)>
                                        .Create((t1, t2) => t1.NextRun == t2.NextRun ? 0
                                                          : t1.NextRun == null ? -1
                                                          : t1.NextRun.Value > t2.NextRun.Value ? 1 : -1);

                    while (!_stopCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var timeSnapshot = _configuration.DatetimeAccessor();
                        for (int i = 0; i < tuples.Length; ++i)
                        {
                            tuples[i] = (pipelines[i].GetNextRun(prevRun), pipelines[i]);
                        }
                        if (_stopCancellationTokenSource.Token.IsCancellationRequested)
                            break;

                        Array.Sort(tuples, tupleComparer);

                        if (_stopCancellationTokenSource.Token.IsCancellationRequested)
                            break;

                        for (int i = 0; i < tuples.Length; ++i)
                        {
                            var (nextRun, pipeline) = tuples[i];
                            if (nextRun == null)
                                break;

                            if (nextRun.Value < timeSnapshot)
                            {
                                var context = new TaskContext(_configuration.ServiceProvider);
                                var buildedPipelineFunc = pipeline.Build();
                                _ = buildedPipelineFunc(context).ConfigureAwait(false);
                            }
                        }
                        // we don't want miss any pipelines run
                        prevRun = timeSnapshot;
                        // Task.Delay use Timer + create memory traffic, so we just block thread via CT
                        // See https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,5896
                        _stopCancellationTokenSource.Token.WaitHandle.WaitOne(1000);
                    }
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public Curator(ICuratorConfiguration configuration) => _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
}