using System;

namespace ResourceCurator.Github
{

    public class GithubGraphQlResourceProducerSettings : CronResourceProducerSettings
    {
        public string ResourceName { get; set; }
        public string Token { get; set; }
        public string Query { get; set; }
        public string UserAgent { get; set; }

    }
}