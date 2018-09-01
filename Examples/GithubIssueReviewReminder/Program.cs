using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using Serilog.Extensions.Logging;
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

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var accessor = serviceProvider.GetRequiredService<IResourceProducerAccessor>();
            var producer = accessor.GetProducer<GithubGraphQlResource<QueryResults>>("GitHubPullRequests");
            var logger = serviceProvider.GetService<ILogger<Program>>();
            using (producer.Resource.Subscribe(onNext: queryResult =>
            {
                var searchResult = queryResult.Value.Data.Search;
                logger.LogInformation("Opened pull requests count: {IssueCount}", searchResult.IssueCount);
                foreach (var edge in searchResult.Edges)
                {
                    var pr = edge.Node;
                    logger.LogInformation("PR: #{PullRequestNumber} [{Title}] Last updated: {LastEditedAt:dd.MM.yyyy HH:mm}", pr.Number, pr.Title, pr.LastEditedAt.ToLocalTime());
                    var reviews = pr.Reviews.Nodes.Select(r => new { r.Author.Login, r.State, r.UpdatedAt }).Union(pr.ReviewRequests.Nodes.Select(r => new { r.RequestedReviewer.Login, State = "Unreviewed", UpdatedAt = DateTimeOffset.MinValue }));
                    foreach(var review in reviews)
                    {
                        if (review.UpdatedAt == DateTimeOffset.MinValue)
                            logger.LogWarning("{Login} - response {PullRequestState} pull request", review.Login  ?? "null", "NOTSEEN");
                        else
                            logger.LogInformation("{Login} response {PullRequestState} pull request at {ApproveDateTime:dd.MM.yyyy HH:mm}", review.Login  ?? "null", review.State ?? "null", review.UpdatedAt.ToLocalTime());
                    }

                }

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
                throw new Exception("Repository argument has incorrect format");

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
          reviews(first: 100, states: APPROVED) {
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