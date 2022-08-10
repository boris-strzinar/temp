using CommandLine;
using MetaExchange;
using MetaExchange.Models;
using Newtonsoft.Json;

namespace MetaExchangeCLI
{
    internal class ConsoleApplication
    {
        // CLI options definition
        private class Options
        {
            [Option('o', "order-books", Required = true, HelpText = "Order books data file path.")]
            public string? OrderBooksDataFile { get; set; }

            [Option('b', "buy", HelpText = "Amount to buy.", SetName = "buy")]
            public double BuyAmount { get; set; }

            [Option('s', "sell", HelpText = "Amount to sell.", SetName = "sell")]
            public double SellAmount { get; set; }

            [Option(Default = 0, Required = false, HelpText = "Order books data limit.")]
            public long Limit { get; set; }
        }


        private IOrderBookService _orderBookService;

        public ConsoleApplication(IOrderBookService orderBookService)
        {
            _orderBookService = orderBookService;
        }

        public void Run(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
        }

        private void RunOptions(Options opts)
        {
            if (!File.Exists(opts.OrderBooksDataFile))
            {
                Console.WriteLine("{0} is not a valid file.", opts.OrderBooksDataFile);
                return;
            }

            var err = _orderBookService.ReadOrderBookDataFile(opts.OrderBooksDataFile, opts.Limit);
            if (err != null)
            {
                Console.WriteLine(err);
                return;
            }

            if (opts.BuyAmount > 0)
            {
                (var remainingAmount, var myOrders) = _orderBookService.Buy(opts.BuyAmount);

                Console.WriteLine(JsonConvert.SerializeObject(myOrders, Formatting.Indented));
                if (remainingAmount > 0.0)
                {
                    Console.WriteLine("Balance too low to make the requested buy order(s). Amount remaining: {0}", remainingAmount);
                }
            }
            else if (opts.SellAmount > 0)
            {
                (var remainingAmount, var myOrders) = _orderBookService.Sell(opts.SellAmount);

                Console.WriteLine(JsonConvert.SerializeObject(myOrders, Formatting.Indented));
                if (remainingAmount > 0.0)
                {
                    Console.WriteLine("Balance too low to make the requested sell order(s). Amount remaining: {0}", remainingAmount);
                }
            }
        }
    }
}
