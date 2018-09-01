using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ResourceCurator;

namespace GithubIssueReviewReminder
{
    public class QueryResults
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("search")]
        public Search Search { get; set; }
    }

    public class Search
    {
        [JsonProperty("issueCount")]
        public long IssueCount { get; set; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; set; }

        [JsonProperty("edges")]
        public List<Edge> Edges { get; set; }
    }

    public class Edge
    {
        [JsonProperty("node")]
        public EdgeNode Node { get; set; }
    }

    public class EdgeNode
    {
        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("lastEditedAt")]
        public DateTimeOffset LastEditedAt { get; set; }

        [JsonProperty("reviews")]
        public Reviews Reviews { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("reviewRequests")]
        public ReviewRequests ReviewRequests { get; set; }
    }

    public class Author
    {
        [JsonProperty("login")]
        public string Login { get; set; }
    }

    public class ReviewRequests
    {
        [JsonProperty("nodes")]
        public List<ReviewRequestsNode> Nodes { get; set; }
    }

    public class ReviewRequestsNode
    {
        [JsonProperty("requestedReviewer")]
        public Author RequestedReviewer { get; set; }
    }

    public class Reviews
    {
        [JsonProperty("nodes")]
        public List<ReviewsNode> Nodes { get; set; }
    }

    public class ReviewsNode
    {
        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class PageInfo
    {
        [JsonProperty("startCursor")]
        public string StartCursor { get; set; }

        [JsonProperty("endCursor")]
        public string EndCursor { get; set; }

        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }
    }
}


