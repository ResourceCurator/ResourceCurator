using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;

namespace ResourceCurator.Github
{
    public class GithubGraphQlQuery {
        [JsonProperty("query")]
        public string Query { get; set; }
    }

    /// <summary>
    /// Endpoint https://api.github.com/graphql
    /// </summary>
    // we cant use defaults because it's must be configured by settings
    [Headers("Authorization: Bearer",
             "User-Agent: ResourceCurator.Github",
             "Accept: application/vnd.github.v3+json")]
    public interface IGitHubGraphQlApi
    {
        [Post("/graphql")]
        Task<TResult> Query<TResult>([Body(BodySerializationMethod.Json)] GithubGraphQlQuery query, [Header("User-Agent")] string userAgent, [Header("Authorization")] string auth);
    }
}