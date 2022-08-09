using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MetaExchangeCLI
{
    internal class Program
    {

        public class Options
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
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);

        }

        public class OrderBook
        {
            public DateTime AcqTime { get; set; }
            public List<OrderEnvelope> Asks { get; set; }
            public List<OrderEnvelope> Bids { get; set; }
        }

        public class OrderEnvelope
        {
            public Order Order { get; set; }
        }

        public class Order
        {
            public enum OrderType { Buy, Sell }

            public enum OrderKind { Unknown, Limit }

            public string? Id { get; set; }
            public DateTime Time { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public OrderType Type { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public OrderKind Kind { get; set; }
            public double Amount { get; set; }
            public double Price { get; set; }

            [JsonIgnore]
            public ExchangeInfo? MyExchangeInfo { get; set; }
        }

        public class ExchangeInfo
        {
            public double BalanceBTC { get; set; }
            public double BalanceEUR { get; set; }
        }

        public class OrderPriorityQueue : PriorityQueue<Order, Order>
        {
            class OrderComparer : IComparer<Order>
            {
                public int Compare(Order? o1, Order? o2)
                {
                    // first check balance constraints
                    bool orderPossible1, orderPossible2;
                    if (o1?.Type == Order.OrderType.Buy)
                    {
                        orderPossible1 = o1?.MyExchangeInfo?.BalanceEUR > 0.0;
                        orderPossible2 = o2?.MyExchangeInfo?.BalanceEUR > 0.0;
                    }
                    else
                    {
                        orderPossible1 = o1?.MyExchangeInfo?.BalanceBTC > 0.0;
                        orderPossible2 = o2?.MyExchangeInfo?.BalanceBTC > 0.0;
                    }

                    if (!orderPossible1 && !orderPossible2)
                    {
                        // no order is possible
                        return 0;
                    }
                    else if (!orderPossible1)
                    {
                        return 1;
                    }
                    else if (!orderPossible2)
                    {
                        return -1;
                    }

                    // both orders are possible
                    if (o1.Price < o2.Price)
                    {
                        return o1.Type == Order.OrderType.Buy ? 1 : -1;
                    }
                    else if (o1.Price > o2.Price)
                    {
                        return o1.Type == Order.OrderType.Buy ? -1 : 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public OrderPriorityQueue() : base(new OrderComparer()) { }

            public void EnqueueOrders(IEnumerable<OrderEnvelope> orders, ExchangeInfo exchangeInfo)
            {
                foreach (var o in orders)
                {
                    Order order = o.Order;
                    order.MyExchangeInfo = exchangeInfo;
                    Enqueue(order, order);
                }
            }
        }

        static void RunOptions(Options opts)
        {
            if (!File.Exists(opts.OrderBooksDataFile))
            {
                Console.WriteLine("{0} is not a valid file.", opts.OrderBooksDataFile);
                return;
            }

            var asks = new OrderPriorityQueue();
            var bids = new OrderPriorityQueue();

            int counter = 0;

            // read the file line by line
            foreach (string line in File.ReadLines(opts.OrderBooksDataFile))
            {
                counter++;
                string[] parts = line.Split('\t', 2);
                if (parts.Length != 2)
                {
                    Console.WriteLine("Invalid order book line.");
                    continue;
                }

                // ignore 1st part (timestamp)
                var json = parts[1];

                ExchangeInfo exchangeInfo = new ExchangeInfo { BalanceBTC = 5.0, BalanceEUR = 10000.0 };
                OrderBook orderBook = JsonConvert.DeserializeObject<OrderBook>(json);
                asks.EnqueueOrders(orderBook.Asks, exchangeInfo);
                bids.EnqueueOrders(orderBook.Bids, exchangeInfo);

                if (opts.Limit > 0 && counter >= opts.Limit)
                    break;
            }

            List<Order> myOrders = new List<Order>();
            double amount = 0.0;
            if (opts.BuyAmount > 0)
            {
                amount = opts.BuyAmount;
                do
                {
                    var order = asks.Peek();
                    if (order.MyExchangeInfo.BalanceEUR > 0.0)
                    {
                        var currentOrderAmount = Math.Min(order.Amount, amount);
                        var currentOrderValue = currentOrderAmount * order.Price;
                        if (order.MyExchangeInfo.BalanceEUR < currentOrderValue)
                        {
                            currentOrderAmount = order.MyExchangeInfo.BalanceEUR / order.Price;
                            currentOrderValue = order.MyExchangeInfo.BalanceEUR;
                        }
                        order.MyExchangeInfo.BalanceEUR -= currentOrderValue;
                        order.MyExchangeInfo.BalanceBTC += currentOrderAmount;
                        if (order.Amount > currentOrderAmount)
                        {
                            order.Amount -= currentOrderAmount;
                            amount -= currentOrderAmount;
                        }
                        else
                        {
                            amount -= order.Amount;
                            asks.Dequeue();
                        }

                        myOrders.Add(new Order { Amount = currentOrderAmount, Price = order.Price, Type = Order.OrderType.Buy });
                    }
                    else
                    {
                        // there are no more bids on exchanges we have a positive balance
                        break;
                    }
                }
                while (amount > 0.0);
            }
            else if (opts.SellAmount > 0)
            {
                amount = opts.SellAmount;
                do
                {
                    var order = bids.Peek();
                    if (order.MyExchangeInfo.BalanceBTC > 0.0)
                    {
                        var currentOrderAmount = Math.Min(order.Amount, amount);
                        if (order.MyExchangeInfo.BalanceBTC < currentOrderAmount)
                        {
                            currentOrderAmount = order.MyExchangeInfo.BalanceBTC;
                        }
                        var currentOrderValue = currentOrderAmount * order.Price;

                        order.MyExchangeInfo.BalanceEUR += currentOrderValue;
                        order.MyExchangeInfo.BalanceBTC -= currentOrderAmount;
                        if (order.Amount > currentOrderAmount)
                        {
                            order.Amount -= currentOrderAmount;
                            amount -= currentOrderAmount;
                        }
                        else
                        {
                            amount -= order.Amount;
                            bids.Dequeue();
                        }

                        myOrders.Add(new Order { Amount = currentOrderAmount, Price = order.Price, Type = Order.OrderType.Sell });
                    }
                    else
                    {
                        // there are no more bids on exchanges we have a positive balance
                        break;
                    }
                }
                while (amount > 0.0);
            }

            Console.WriteLine(JsonConvert.SerializeObject(myOrders, Formatting.Indented));

            if (amount > 0.0)
            {
                Console.WriteLine("Balance too low to make the requested order(s). Amount remaining: {0}", amount);
            }
        }
    }
}