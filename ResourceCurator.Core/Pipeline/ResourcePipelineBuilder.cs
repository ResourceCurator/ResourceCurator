using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

namespace ResourceCurator
{
    public sealed class ResourcePipelineBuilder<TResource> : IResourcePipelineBuilder<TResource>
        where TResource: IResource
    {
        internal readonly IServiceScope _scope;
        private readonly ResourcePipeline<TResource> _pipeline;
        private readonly IObservable<TResource> _observable;

        internal readonly CompositeDisposable _disposables;

        internal ResourcePipelineBuilder(ResourcePipeline<TResource> pipeline, IServiceProvider serviceProvider, IObservable<TResource> observable)
        {
            _scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            _pipeline = pipeline;
            _observable = observable;
            _disposables = new CompositeDisposable(2) { _scope };
        }
        
        public IResourcePipelineBuilder<TResource> Task(Func<PipelineTaskDelegate<TResource>, PipelineTaskDelegate<TResource>> pipelineTask)
        {
            if (pipelineTask == null)
                throw new ArgumentNullException(nameof(pipelineTask));

            _pipeline.AddTask(pipelineTask);
            return this;
        }

        public IDisposable Subscribe()
        {
            // subscribe to observable
            var pipeline = _pipeline.Build();
            var scheduler = _scope.ServiceProvider.GetRequiredService<IScheduler>();
            // onNext is void, so our async lambda is bad,
            // we just use SelectMany for async logic here, but in future we can do more:
            // ToDo: add overrides for concurrent execution N resources (via SelectMany)
            // More info:
            // https://github.com/dotnet/reactive/issues/459#issuecomment-357648243
            // https://stackoverflow.com/questions/23006852/howto-call-back-async-function-from-rx-subscribe
            // ToDo: (?) How we can improve this
            var disposable = _observable.Select(resource => Observable.FromAsync(async () => 
            // ToDo: Add pooling to PipeLineContext    
            await pipeline(new PipelineContext<TResource>(_scope.ServiceProvider, resource)).ConfigureAwait(false),
                scheduler
                )).Concat().Publish().RefCount().Subscribe<Unit>(
                // ToDo: Add logging (for exceptions etc, add logic shallow / add delegate for catching them)
                onNext: _ => { },
                onCompleted: () => _disposables.Dispose(),
                onError: ex => throw ex);

            _disposables.Add(disposable);
            
            return _disposables;

            
        }
    }

}
