using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ResourceCurator.Github
{
    public class GithubGraphQlResource<TQueryResult> : Resource<TQueryResult>
        where TQueryResult: new()
    {
        public GithubGraphQlResource(string producerHash, TQueryResult value, string resourceName) : base(producerHash, value)
            => Name = resourceName ?? throw new ArgumentNullException(resourceName);

        public override string Name { get; }
    }
    public class GithubGraphQlResourceProducer<TQueryResult> : CronResourceProducerAsync<GithubGraphQlResource<TQueryResult>, GithubGraphQlResourceProducerSettings>, IResourceProducer<GithubGraphQlResource<TQueryResult>>
         where TQueryResult: class, new()
    {
        private readonly IGitHubGraphQlApi _graphqlApi;
        private readonly string _auth;
        private readonly GithubGraphQlQuery _query;

        public GithubGraphQlResourceProducer(string name, GithubGraphQlResourceProducerSettings settings, ISerializer serializer, IGitHubGraphQlApi graphqlApi) : this(name, settings, serializer, graphqlApi, Scheduler.Default) { }

        public GithubGraphQlResourceProducer(string name, GithubGraphQlResourceProducerSettings settings, ISerializer serializer, IGitHubGraphQlApi graphqlApi, IScheduler scheduler) : base(name, settings, serializer, scheduler)
        {
            _graphqlApi = graphqlApi ?? throw new ArgumentNullException(nameof(graphqlApi));

            if (string.IsNullOrWhiteSpace(Settings.Token))
                throw new ArgumentException("Token must be setted", nameof(settings));

            if (string.IsNullOrWhiteSpace(Settings.CronSchedule))
                throw new ArgumentException("Cron schedule string must be setted", nameof(settings));

            if (string.IsNullOrWhiteSpace(Settings.Query))
                throw new ArgumentException("GraphQL query must be setted", nameof(settings));

            if (string.IsNullOrWhiteSpace(Settings.UserAgent))
                throw new ArgumentException("User-agent must be setted", nameof(settings));

            if (string.IsNullOrWhiteSpace(Settings.ResourceName))
                Settings.ResourceName = "GraphQl unnamed query resource";

            _auth = "Bearer " + Settings.Token;
            _query = new GithubGraphQlQuery() { Query = Settings.Query };
        }

        public override async Task<GithubGraphQlResource<TQueryResult>> PullResourceAsync()
        {
            var queryResult = await _graphqlApi.Query<TQueryResult>(_query, Settings.UserAgent, _auth).ConfigureAwait(false);
            return new GithubGraphQlResource<TQueryResult>(Hash, queryResult, Settings.ResourceName);
        }
    }
}