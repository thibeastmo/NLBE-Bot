using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NLBE_Bot
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("NLBE Bot is starting.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {                    
                    await new Bot(_logger, _configuration).RunAsync(); // Note: the bot does not yet support gracefull cancellation.                 
                }
            }  
            catch (OperationCanceledException)
            {
                Console.WriteLine("NLBE Bot was canceled.");
            }
            finally
            {
                Console.WriteLine("NLBE Bot is stopping.");
            }
        }
    }
}