using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace CartTrackingService
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((host, services) => { services.AddHostedService<TrackingService>(); });

            await builder
                .RunConsoleAsync()
                .ContinueWith(x => Console.Out.WriteLine("Cart Tracking Service initialized"));
        }
    }
}