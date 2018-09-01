using System;
using Microsoft.Extensions.Http;

namespace GithubIssueReviewReminder
{
    public class NullLoggingFilter : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next) => builder => {

            // Run other config first
            next(builder);
            builder.AdditionalHandlers.Clear();
        };
    }
}