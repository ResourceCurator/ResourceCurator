using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ResourceCurator;

namespace Example
{
    public interface IPingService
    {
        Task<bool> PingAsync(string domain);
    }
    public class HttpsPingService : IPingService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpsPingService(IHttpClientFactory httpClientFactory)
        {
            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> PingAsync(string domain)
        {
            var client = _httpClientFactory.CreateClient();
            string response = null;
            try
            {
                response = await client.GetStringAsync("https://" + domain).ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
            return !string.IsNullOrEmpty(response);
        }
    }

    public abstract class HttpsPingTaskBase : IMiddlewareTask
    {
        public abstract string Domain { get; }

        protected virtual void Log(string str)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        private readonly IPingService _pingService;

        protected HttpsPingTaskBase(IPingService pingService)
        {
            if (pingService == null)
                throw new ArgumentNullException(nameof(pingService));

            _pingService = pingService;
        }

        public virtual async Task InvokeAsync(ITaskContext context, TaskDelegate nextMiddleware)
        {
            string log = $"Ping {Domain}...";
            bool isOk = await _pingService.PingAsync(Domain);
            log += isOk ? "Ok" : "Error";
            Log(log);
            context.Items[Domain] = isOk;
            await nextMiddleware(context);
        }
    }

    public class GithubPingTask : HttpsPingTaskBase
    {
        public override string Domain => "github.com";
        public GithubPingTask(IPingService pingService) : base(pingService) { }

    }
    public class GooglePingTask : HttpsPingTaskBase
    {
        public override string Domain => "google.com";
        public GooglePingTask(IPingService pingService) : base(pingService) { }

    }

    public class PingResultsPrinterTask : IMiddlewareTask
    {
        public async Task InvokeAsync(ITaskContext context, TaskDelegate nextMiddleware)
        {
            WriteColor($"Results run at [{DateTime.Now.ToLongTimeString()}]:\n", ConsoleColor.Cyan);
            if(context.Items["github.com"] is bool githubIsOk)
            {
                Console.Write("Github - ");
                WriteStatus(githubIsOk);
                Console.Write("\n");
            }
            if (context.Items["google.com"] is bool googleIsOk)
            {
                Console.Write("Google - ");
                WriteStatus(googleIsOk);
                Console.Write("\n");
            }
            await nextMiddleware(context);
        }

        private static void WriteStatus(bool status)
        {
            if (status)
                WriteColor("[OK]", ConsoleColor.Green);
            else
                WriteColor("[ERROR]", ConsoleColor.Red);
        }

        private static void WriteColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }
    }

    class Program
    {


        public static async Task Main(string[] args)
        {

            var serviceCollection = new ServiceCollection()
                                        .AddHttpClient()
                                        .AddSingleton<IPingService, HttpsPingService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var builder = new CuratorConfigurationBuilder().UseServiceProvider(serviceProvider);
            builder.AddPipeline("PingPipeline")
                .WithSchedule("*/5 * * * * *")
                .Task<GithubPingTask>()
                .Task<GooglePingTask>()
                .Task<PingResultsPrinterTask>();

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) => {
                Console.WriteLine("\nStop executing...");
                cts.Cancel();
                Thread.Sleep(1000);
                 Environment.Exit(0);
                
            };
            Console.WriteLine("Press `Ctrl + C` to stop...");
            Console.WriteLine($"Next run at {DateTime.Now.AddSeconds(5).ToLongTimeString()}");

            using (ICurator overseer = new Curator(builder.Build()))
                await overseer.StartAsync(cts.Token).ConfigureAwait(false);

            Console.WriteLine("Press `Enter` to exit...");
            Console.ReadLine();
        }
    }
}
