using MetaExchange;
using MetaExchange.Models;

namespace MetaExchangeTests
{
    public class OrderBookServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestBuyNoOrders()
        {
            OrderBookService orderBookService = new OrderBookService();
            var (remaining, orders) = orderBookService.Buy(1.0);
            Assert.AreEqual(1.0, remaining, 0.0001);
            Assert.IsEmpty(orders);
        }

        [Test]
        public void TestBuyNoBalance()
        {
            var prices = new double[] { 1000.0, 3000.0, 2000.0 };
            var asks = new OrderPriorityQueue();

            var exchangeInfo = new ExchangeInfo { BalanceBTC = 2.0, BalanceEUR = 0.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Sell, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo };
                asks.Enqueue(order, order);
            }

            var orderBookService = new OrderBookService(asks, new OrderPriorityQueue());

            var (remaining, orders) = orderBookService.Buy(1.0);
            Assert.AreEqual(1.0, remaining, 0.0001);
            Assert.IsEmpty(orders);
        }

        [Test]
        public void TestBuyLowBalance()
        {
            var prices = new double[] { 1000.0, 3000.0, 2000.0 };
            var asks = new OrderPriorityQueue();

            var exchangeInfo = new ExchangeInfo { BalanceBTC = 2.0, BalanceEUR = 2000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Sell, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo };
                asks.Enqueue(order, order);
            }

            var orderBookService = new OrderBookService(asks, new OrderPriorityQueue());

            var (remaining, orders) = orderBookService.Buy(2.0);
            Assert.AreEqual(2, orders.Count());
            Assert.AreEqual(1000.0, orders.ElementAt(0).Price);
            Assert.AreEqual(1.0, orders.ElementAt(0).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Buy, orders.ElementAt(0).Type);
            Assert.AreEqual(2000.0, orders.ElementAt(1).Price);
            Assert.AreEqual(0.5, orders.ElementAt(1).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Buy, orders.ElementAt(1).Type);
            Assert.AreEqual(0.5, remaining, 0.0001);
        }

        [Test]
        public void TestBuy()
        {
            var prices = new double[] { 1000.0, 3000.0, 2000.0 };
            var asks = new OrderPriorityQueue();

            var exchangeInfo = new ExchangeInfo { BalanceBTC = 2.0, BalanceEUR = 5000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Sell, Amount = 2.0, Price = price, MyExchangeInfo = exchangeInfo };
                asks.Enqueue(order, order);
            }

            var orderBookService = new OrderBookService(asks, new OrderPriorityQueue());

            var (remaining, orders) = orderBookService.Buy(3.0);
            Assert.AreEqual(2, orders.Count());
            Assert.AreEqual(1000.0, orders.ElementAt(0).Price);
            Assert.AreEqual(2.0, orders.ElementAt(0).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Buy, orders.ElementAt(0).Type);
            Assert.AreEqual(2000.0, orders.ElementAt(1).Price);
            Assert.AreEqual(1.0, orders.ElementAt(1).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Buy, orders.ElementAt(1).Type);
            Assert.AreEqual(0.0, remaining, 0.0001);
            Assert.AreEqual(1000.0, exchangeInfo.BalanceEUR, 0.0001);

            // check next best ask
            var ask = asks.Peek();
            Assert.AreEqual(2000.0, ask.Price, 0.0001);
            Assert.AreEqual(1.0, ask.Amount, 0.0001);
        }

        [Test]
        public void TestBuyDifferentExchanges()
        {
            var prices = new double[] { 1000.0, 3000.0, 2000.0 };
            var asks = new OrderPriorityQueue();

            var exchangeInfo1 = new ExchangeInfo { BalanceBTC = 2.0, BalanceEUR = 2000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Sell, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo1 };
                asks.Enqueue(order, order);
            }

            var exchangeInfo2 = new ExchangeInfo { BalanceBTC = 2.0, BalanceEUR = 10000.0 };
            var order2 = new ExchangeOrder { Type = ExchangeOrder.OrderType.Sell, Amount = 10.0, Price = 4000.0, MyExchangeInfo = exchangeInfo2 };
            asks.Enqueue(order2, order2);

            var orderBookService = new OrderBookService(asks, new OrderPriorityQueue());

            var (remaining, orders) = orderBookService.Buy(3.0);
            Assert.AreEqual(3, orders.Count());
            Assert.AreEqual(1000.0, orders.ElementAt(0).Price);
            Assert.AreEqual(1.0, orders.ElementAt(0).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Buy, orders.ElementAt(0).Type);
            Assert.AreEqual(2000.0, orders.ElementAt(1).Price);
            Assert.AreEqual(0.5, orders.ElementAt(1).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Buy, orders.ElementAt(1).Type);
            Assert.AreEqual(4000.0, orders.ElementAt(2).Price);
            Assert.AreEqual(1.5, orders.ElementAt(2).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Buy, orders.ElementAt(2).Type);
            Assert.AreEqual(0.0, remaining, 0.0001);

            Assert.AreEqual(0.0, exchangeInfo1.BalanceEUR, 0.0001);
            Assert.AreEqual(4000.0, exchangeInfo2.BalanceEUR, 0.0001);

            // check next best ask
            var ask = asks.Peek();
            Assert.AreEqual(4000.0, ask.Price, 0.0001);
            Assert.AreEqual(8.5, ask.Amount, 0.0001);
        }

        [Test]
        public void TestSellNoOrders()
        {
            OrderBookService orderBookService = new OrderBookService();
            var (remaining, orders) = orderBookService.Sell(1.0);
            Assert.AreEqual(1.0, remaining, 0.0001);
            Assert.IsEmpty(orders);
        }

        [Test]
        public void TestSellNoBalance()
        {
            var prices = new double[] { 1000.0, 3000.0, 2000.0 };
            var bids = new OrderPriorityQueue();

            var exchangeInfo = new ExchangeInfo { BalanceBTC = 0.0, BalanceEUR = 2000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Buy, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo };
                bids.Enqueue(order, order);
            }

            var orderBookService = new OrderBookService(new OrderPriorityQueue(), bids);

            var (remaining, orders) = orderBookService.Sell(1.0);
            Assert.AreEqual(1.0, remaining, 0.0001);
            Assert.IsEmpty(orders);
        }

        [Test]
        public void TestSellLowBalance()
        {
            var prices = new double[] { 1000.0, 3000.0, 2000.0 };
            var bids = new OrderPriorityQueue();

            var exchangeInfo = new ExchangeInfo { BalanceBTC = 2.0, BalanceEUR = 2000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Buy, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo };
                bids.Enqueue(order, order);
            }

            var orderBookService = new OrderBookService(new OrderPriorityQueue(), bids);

            var (remaining, orders) = orderBookService.Sell(3.0);
            Assert.AreEqual(2, orders.Count());
            Assert.AreEqual(3000.0, orders.ElementAt(0).Price);
            Assert.AreEqual(1.0, orders.ElementAt(0).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Sell, orders.ElementAt(0).Type);
            Assert.AreEqual(2000.0, orders.ElementAt(1).Price);
            Assert.AreEqual(1.0, orders.ElementAt(1).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Sell, orders.ElementAt(1).Type);
            Assert.AreEqual(1.0, remaining, 0.0001);
            Assert.AreEqual(0.0, exchangeInfo.BalanceBTC, 0.0001);
        }

        [Test]
        public void TestSell()
        {
            var prices = new double[] { 1000.0, 3000.0, 2000.0 };
            var bids = new OrderPriorityQueue();

            var exchangeInfo = new ExchangeInfo { BalanceBTC = 5.0, BalanceEUR = 2000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Buy, Amount = 2.0, Price = price, MyExchangeInfo = exchangeInfo };
                bids.Enqueue(order, order);
            }

            var orderBookService = new OrderBookService(new OrderPriorityQueue(), bids);

            var (remaining, orders) = orderBookService.Sell(3.0);
            Assert.AreEqual(2, orders.Count());
            Assert.AreEqual(3000.0, orders.ElementAt(0).Price);
            Assert.AreEqual(2.0, orders.ElementAt(0).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Sell, orders.ElementAt(0).Type);
            Assert.AreEqual(2000.0, orders.ElementAt(1).Price);
            Assert.AreEqual(1.0, orders.ElementAt(1).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Sell, orders.ElementAt(1).Type);
            Assert.AreEqual(0.0, remaining, 0.0001);
            Assert.AreEqual(2.0, exchangeInfo.BalanceBTC, 0.0001);

            // check next best bid
            var bid = bids.Peek();
            Assert.AreEqual(2000.0, bid.Price, 0.0001);
            Assert.AreEqual(1.0, bid.Amount, 0.0001);
        }

        [Test]
        public void TestSellDifferentExchanges()
        {
            var prices = new double[] { 1000.0, 3000.0, 2000.0 };
            var bids = new OrderPriorityQueue();

            var exchangeInfo1 = new ExchangeInfo { BalanceBTC = 3.0, BalanceEUR = 2000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Buy, Amount = 2.0, Price = price, MyExchangeInfo = exchangeInfo1 };
                bids.Enqueue(order, order);
            }

            var exchangeInfo2 = new ExchangeInfo { BalanceBTC = 10.0, BalanceEUR = 10000.0 };
            var order2 = new ExchangeOrder { Type = ExchangeOrder.OrderType.Buy, Amount = 10.0, Price = 500.0, MyExchangeInfo = exchangeInfo2 };
            bids.Enqueue(order2, order2);

            var orderBookService = new OrderBookService(new OrderPriorityQueue(), bids);

            var (remaining, orders) = orderBookService.Sell(5.0);
            Assert.AreEqual(3, orders.Count());
            Assert.AreEqual(3000.0, orders.ElementAt(0).Price);
            Assert.AreEqual(2.0, orders.ElementAt(0).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Sell, orders.ElementAt(0).Type);
            Assert.AreEqual(2000.0, orders.ElementAt(1).Price);
            Assert.AreEqual(1.0, orders.ElementAt(1).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Sell, orders.ElementAt(1).Type);
            Assert.AreEqual(500.0, orders.ElementAt(2).Price);
            Assert.AreEqual(2.0, orders.ElementAt(2).Amount, 0.0001);
            Assert.AreEqual(ExchangeOrder.OrderType.Sell, orders.ElementAt(2).Type);
            Assert.AreEqual(0.0, remaining, 0.0001);

            Assert.AreEqual(0.0, exchangeInfo1.BalanceBTC, 0.0001);
            Assert.AreEqual(8.0, exchangeInfo2.BalanceBTC, 0.0001);

            // check next best bid
            var ask = bids.Peek();
            Assert.AreEqual(500.0, ask.Price, 0.0001);
            Assert.AreEqual(8.0, ask.Amount, 0.0001);
        }
    }
}