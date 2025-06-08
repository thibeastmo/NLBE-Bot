using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NLBE_Bot
{
    public class BackgroundWorker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("NLBE Bot is starting.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {                    
                    await new Bot().RunAsync(); // Note: the bot does not yet support gracefull cancellation.                 
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