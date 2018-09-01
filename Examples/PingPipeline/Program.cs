using System;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ResourceCurator;
using ObservableExtensions = ResourceCurator.ObservableExtensions;
using ResourceCurator.Serialization.Yaml;

namespace Example
{


    internal class Program
    {
        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                                        .AddHttpClient()
                                        .AddScheduler()
                                        .AddSingleton<ISerializer, YamlSerializer>()
                                        .AddResourceProducer<DomainName, RandomDomainNameProducer>(
                                            "RandomDomainGenerator",
                                            new CronResourceProducerSettings(){ CronSchedule = "*/5 * * * * *" })
                                        .AddSingleton<IResourceProducerAccessor, ResourceProducerAccessor>()
                                        .AddSingleton<IPingService, HttpsPingService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var accessor = serviceProvider.GetRequiredService<IResourceProducerAccessor>();

            Console.WriteLine("Press `Ctrl + C` to stop...");
            Console.WriteLine($"Started at {DateTime.Now:HH:mm:ss.fff}");
            Console.WriteLine("Press `Enter` to exit...");
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("\nStop executing...");
                Environment.Exit(0);
            };

            using (var subscribe = accessor.GetProducer<DomainName>("RandomDomainGenerator")
                    .Resource
                    .Pipeline("Ping pipeline", serviceProvider)
                    .Task<DomainName, HttpsPingTask>()
                    .Task<DomainName, PingResultsPrinterTask>().Subscribe())
            {
                Console.ReadLine();
            }
        }
    }

    public class DomainName : Resource<string>
    {
        public override string Name => "Random domain name from list";

        public DomainName(string producerHash, string value) : base(producerHash, value)
        {
        }
    }

    public class RandomDomainNameProducer : CronResourceProducer<DomainName>
    {
        private readonly string[] _domainNames = new []{"google.com", "github.com"};
        protected static Random _rand = new Random(DateTime.UtcNow.Millisecond * DateTime.UtcNow.Second * 317);

        // This simulate pull some resource, for example getting sql server free memory
        public override DomainName PullResource() => new DomainName(Name, _domainNames[_rand.Next(_domainNames.Length)]);

        public RandomDomainNameProducer(string name, CronResourceProducerSettings settings, ISerializer serializer)
                    : base(name, settings, serializer) { }

        public RandomDomainNameProducer(string name, CronResourceProducerSettings settings, ISerializer serializer, IScheduler scheduler)
            : base(name, settings, serializer, scheduler) { }
    }
    public class HttpsPingTask : IPipelineTask<DomainName>
    {
        private readonly IPingService _pingService;

        protected virtual void Log(string str)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        public virtual async Task InvokeAsync(IPipelineContext<DomainName> context, PipelineTaskDelegate<DomainName> nextMiddleware)
        {
            string log = $"Ping {context.Resource.Value}...";
            bool isOk = await _pingService.PingAsync(context.Resource.Value);
            log += isOk ? "Ok" : "Error";
            Log(log);
            context.Items[context.Resource.Value] = isOk;
            await nextMiddleware(context);
        }

        public HttpsPingTask(IPingService pingService)
        {
            if (pingService == null)
                throw new ArgumentNullException(nameof(pingService));

            _pingService = pingService;
        }
    }

    public class PingResultsPrinterTask : IPipelineTask<DomainName>
    {
        private static void WriteColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        private static void WriteStatus(bool status)
        {
            if (status)
                WriteColor("[OK]", ConsoleColor.Green);
            else
                WriteColor("[ERROR]", ConsoleColor.Red);
        }

        public async Task InvokeAsync(IPipelineContext<DomainName> context, PipelineTaskDelegate<DomainName> nextMiddleware)
        {
            WriteColor($"Results run at [{DateTime.Now:HH:mm:ss.fff}]:\n", ConsoleColor.Cyan);
            if (context.Resource.Value == "github.com")
            {
                Console.Write("Github - ");
                WriteStatus((bool) context.Items["github.com"]);
                Console.Write("\n");
            }
            if (context.Resource.Value == "google.com")
            {
                Console.Write("Google - ");
                WriteStatus((bool) context.Items["google.com"]);
                Console.Write("\n");
            }
            await nextMiddleware(context);
        }
    }


    public interface IPingService
    {
        Task<bool> PingAsync(string domain);
    }
    public class HttpsPingService : IPingService
    {
        private readonly IHttpClientFactory _httpClientFactory;

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

        public HttpsPingService(IHttpClientFactory httpClientFactory)
        {
            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _httpClientFactory = httpClientFactory;
        }
    }


}