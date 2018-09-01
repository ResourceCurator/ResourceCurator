using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Refit;
using ResourceCurator;
using ResourceCurator.Github;
using ResourceCurator.Serialization.Yaml;
using Serilog;
namespace GithubIssueReviewReminder
{

    public class Program
    {

        public static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Required]
        [Option(Description = "Github API token with `repo` rights", LongName = "token", ShortName = "t", ShowInHelpText = true)]
        public string Token { get; }

        [Required]
        [Option(Description = "Username to track him pull requests", LongName = "username", ShortName = "u", ShowInHelpText = true)]
        public string Username { get; }

        [Required]
        [Option(Description = "Repository, for example ResourceCurator/ResourceCurator", LongName = "repository", ShortName = "r", ShowInHelpText = true)]
        public string Repository { get; }

        [Required]
        [Option(Description = "Slack incoming webhook url", LongName = "slack", ShortName = "s", ShowInHelpText = true)]
        public string SlackWebHookUrl { get; }

        private void OnExecute()
        {
            Init();
            var serviceCollection = new ServiceCollection()
                .AddHttpClient()
                .AddScheduler()
                .AddSingleton<ISerializer, YamlSerializer>()
                .AddLogging(c => c.AddSerilog())
                .AddResourceProducer<GithubGraphQlResource<QueryResults>, GithubGraphQlResourceProducer<QueryResults>>(
                    "GitHubPullRequests",
                    new GithubGraphQlResourceProducerSettings() {
                        CronSchedule = "15 * * * * *",
                        Token = Token,
                        ResourceName = "Pull requests with reviewers",
                        UserAgent = "ResourceCurator-Example",
                        Query = _graphQlQuery.Replace("@({Username})", Username).Replace("@({Repository})", Repository),

                    })
                .AddSingleton<IResourceProducerAccessor, ResourceProducerAccessor>();

            // We want omit logging from HttpClientFactory
            serviceCollection.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, NullLoggingFilter>());

            serviceCollection.AddRefitClient<IGitHubGraphQlApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.github.com"));
            serviceCollection.AddRefitClient<ISlackApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://hooks.slack.com"));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var accessor = serviceProvider.GetRequiredService<IResourceProducerAccessor>();
            var producer = accessor.GetProducer<GithubGraphQlResource<QueryResults>>("GitHubPullRequests");
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var slack = serviceProvider.GetRequiredService<ISlackApi>();
            var parts = SlackWebHookUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            (string webhook1, string webhook2, string webhook3) = (parts[3],parts[4],parts[5]);

            using (producer.Resource
                           .Do(onNext: queryResult => logger.LogInformation("Opened pull requests count: {IssueCount}", queryResult.Value.Data.Search.IssueCount))
                           .SelectMany(queryResult => queryResult.Value.Data.Search.Edges
                                   .Select(edge => (PullRequest: new PullRequest() {
                                       Number = edge.Node.Number,
                                       Title = edge.Node.Title,
                                       Url = edge.Node.Url,
                                       LastUpdatedAt = edge.Node.LastEditedAt,
                                   }, Info: edge.Node))
                                   .SelectMany(tuple => tuple.Info.Reviews.Nodes
                                        .Select(r => new Review() {
                                            Username = r.Author.Login,
                                            LastUpdatedAt = r.UpdatedAt,
                                            PullRequest = tuple.PullRequest,
                                            State = Enum.Parse<ReviewStateEnum>(r.State?.Replace("_", "") ?? "Pending", true)
                                        }).GroupBy(r => r.Username).Select(g => g.OrderByDescending(i => i.LastUpdatedAt).First())
                                        .Union(tuple.Info.ReviewRequests.Nodes
                                            .Select(r => new Review() {
                                                Username = r.RequestedReviewer.Login,
                                                LastUpdatedAt = null,
                                                PullRequest = tuple.PullRequest,
                                                State = ReviewStateEnum.Pending
                                            })))
                           )
                           .Where(r => !string.Equals(r.Username, Username, StringComparison.OrdinalIgnoreCase))
                           .Do(r => logger.LogInformation(
                              "Review of #{PullRequestNumber} {PullRequestTitle} User: {Username} with review status:{State} last updated: {LastUpdated:dd.MM.yyyy HH:mm}",
                              r.PullRequest.Number,
                              r.PullRequest.Title,
                              r.Username,
                              r.State,
                              r.LastUpdatedAt))
                            .Subscribe(onNext: r =>
                            {
                                var json = slack.SendMessage(webhook1, webhook2, webhook3, new SlackMessage(){ Text = $"{r.PullRequest.Url} {r.Username} - [{r.State}] updated: {r.LastUpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "null"}"}).ConfigureAwait(false).GetAwaiter().GetResult();
                                logger.LogDebug("Reminder sended");
                            }))
            {
                Console.ReadLine();
            }

        }

        private void Init()
        {

            Serilog.Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Debug()
                         .WriteTo.Console(Serilog.Events.LogEventLevel.Debug,
                                          outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                         .CreateLogger();



            if (Repository.Split('/').Length < 2)
                throw new Exception("Repository has incorrect format");

            if (SlackWebHookUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length < 6)
                throw new Exception("Slack webhook url has incorrect format");

            Log.Logger.Information("Press `Ctrl + C` or Enter to exit...");
            Log.Logger.Information("Started");
            Console.CancelKeyPress += (s, e) =>
            {
                Log.Logger.Information("\nStop executing...");
                Environment.Exit(0);
            };
        }

        private const string _graphQlQuery = @"{
  search(first: 20, type: ISSUE, query: ""type:pr state:open author:@({Username}) repo:@({Repository})"") {
    issueCount
    pageInfo {
      startCursor
      endCursor
      hasNextPage
    }
    edges {
      node {
        ... on PullRequest {
          author {
            login
          }
          url
          number
          title
          lastEditedAt
          reviews(last: 100) {
            nodes {
              author {
                login
              }
              state
              updatedAt
            }
          }
          updatedAt
          reviewRequests(first: 100) {
            nodes {
              requestedReviewer {
                ... on User {
                  login
                }
              }
            }
          }
        }
      }
    }
  }
}
";
    }



}