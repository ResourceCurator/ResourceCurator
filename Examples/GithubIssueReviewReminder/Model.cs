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
    // Since we send messages to the reviewers, it is more convenient 
    // for us to deal with such a model Review -> PullRequest
    public class Review
    {
        public PullRequest PullRequest { get; set; }
        public string Username { get; set; }
        public DateTimeOffset? LastUpdatedAt { get; set; }
        public ReviewStateEnum State { get; set; }
    }
    public class PullRequest
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public long Number  { get; set; }
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
    public enum ReviewStateEnum
    {
        Pending,
        Commented,
        Approved,
        ChangesRequested,
        Dismissed,
    }



}