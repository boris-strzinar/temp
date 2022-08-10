using MetaExchange;
using Microsoft.Extensions.DependencyInjection;

namespace MetaExchangeCLI
{
    internal class Program
    {


        static void Main(string[] args)
        {
            var services = ConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<ConsoleApplication>().Run(args);
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IOrderBookService, OrderBookService>();
            services.AddSingleton<ConsoleApplication>();
            return services;
        }
    }
}