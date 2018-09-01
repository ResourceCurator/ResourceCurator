using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;

namespace GithubIssueReviewReminder
{
    public interface ISlackApi
    {
        // Refit doesn't support nonserialized query parameter, this easiest workaround
        // https://github.com/reactiveui/refit/issues/277
        [Post("/services/{webhook1}/{webhook2}/{webhook3}")]
        Task<string> SendMessage(string webhook1, string webhook2, string webhook3, [Body(BodySerializationMethod.Json)] SlackMessage message);
    }

    public class SlackMessage
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
