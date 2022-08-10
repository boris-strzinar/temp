﻿using MetaExchange.Models;
using Newtonsoft.Json;

namespace MetaExchange
{
    public class OrderBookService : IOrderBookService
    {
        private OrderPriorityQueue _asks = new OrderPriorityQueue();
        private OrderPriorityQueue _bids = new OrderPriorityQueue();

        public OrderPriorityQueue GetBids() { return _bids; }

        public string? ReadOrderBookDataFile(string path, long limit)
        {
            if (!File.Exists(path))
            {
                return $"'{path}' is not a valid file path.";
            }

            int counter = 0;

            // read the file line by line
            foreach (string line in File.ReadLines(path))
            {
                counter++;
                string[] parts = line.Split('\t', 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                // ignore 1st part (timestamp)
                var json = parts[1];

                ExchangeInfo exchangeInfo = new ExchangeInfo { BalanceBTC = 5.0, BalanceEUR = 10000.0 };
                ExchangeOrderBook orderBook = JsonConvert.DeserializeObject<ExchangeOrderBook>(json);
                _asks.EnqueueOrders(orderBook.Asks, exchangeInfo);
                _bids.EnqueueOrders(orderBook.Bids, exchangeInfo);

                if (limit > 0 && counter >= limit)
                    break;
            }

            return null;
        }

        public (double, IEnumerable<ExchangeOrder>) Buy(double amount)
        {
            List<ExchangeOrder> orders = new List<ExchangeOrder>();

            do
            {
                var order = _asks.Peek();
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
                        _asks.Dequeue();
                    }

                    orders.Add(new ExchangeOrder { Amount = currentOrderAmount, Price = order.Price, Type = ExchangeOrder.OrderType.Buy });
                }
                else
                {
                    // there are no more bids on exchanges we have a positive balance
                    break;
                }
            }
            while (amount > 0.0);

            return (amount, orders);
        }

        public (double, IEnumerable<ExchangeOrder>) Sell(double amount)
        {
            List<ExchangeOrder> orders = new List<ExchangeOrder>();

            do
            {
                var order = _bids.Peek();
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
                        _bids.Dequeue();
                    }

                    orders.Add(new ExchangeOrder { Amount = currentOrderAmount, Price = order.Price, Type = ExchangeOrder.OrderType.Sell });
                }
                else
                {
                    // there are no more bids on exchanges we have a positive balance
                    break;
                }
            }
            while (amount > 0.0);

            return (amount, orders);
        }
    }
}