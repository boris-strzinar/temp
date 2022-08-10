using MetaExchange;
using MetaExchange.Models;

namespace MetaExchangeTests
{
    public class OrderPriorityQueueTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestBuysOrderQueue()
        {
            var prices = new double[] { 1000.0, 2000.0, 1500.0 };
            var buys = new OrderPriorityQueue();

            var exchangeInfo = new ExchangeInfo { BalanceBTC = 2.0, BalanceEUR = 3000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Buy, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo };
                buys.Enqueue(order, order);
            }

            // buy orders should be returned in descending order
            var order1 = buys.Dequeue();
            Assert.AreEqual(2000.0, order1.Price);

            var order2 = buys.Dequeue();
            Assert.AreEqual(1500.0, order2.Price);

            var order3 = buys.Dequeue();
            Assert.AreEqual(1000.0, order3.Price);
        }

        [Test]
        public void TestBuysOrderQueueDifferentExchanges()
        {
            var prices = new double[] { 1000.0, 2000.0, 1500.0 };
            var buys = new OrderPriorityQueue();

            foreach (var price in prices)
            {
                var exchangeInfo = new ExchangeInfo { BalanceBTC = (price < 2000 && price > 1000) ? 0.0 : 0.5, BalanceEUR = 500.0 };
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Buy, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo };
                buys.Enqueue(order, order);
            }

            // buy orders should be returned in descending order, except orders with exchanges with insufficient balance - those are returned last
            var order1 = buys.Dequeue();
            Assert.AreEqual(2000.0, order1.Price);
            Assert.AreEqual(0.5, order1.MyExchangeInfo.BalanceBTC);

            var order2 = buys.Dequeue();
            Assert.AreEqual(1000.0, order2.Price);
            Assert.AreEqual(0.5, order2.MyExchangeInfo.BalanceBTC);

            var order3 = buys.Dequeue();
            Assert.AreEqual(1500.0, order3.Price);
            Assert.AreEqual(0.0, order3.MyExchangeInfo.BalanceBTC);
        }

        [Test]
        public void TestAsksOrderQueue()
        {
            var prices = new double[] { 1000.0, 2000.0, 1500.0 };
            var asks = new OrderPriorityQueue();

            var exchangeInfo = new ExchangeInfo { BalanceBTC = 2.0, BalanceEUR = 3000.0 };
            foreach (var price in prices)
            {
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Sell, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo };
                asks.Enqueue(order, order);
            }

            // buy orders should be returned in ascending order
            var order1 = asks.Dequeue();
            Assert.AreEqual(1000.0, order1.Price);

            var order2 = asks.Dequeue();
            Assert.AreEqual(1500.0, order2.Price);

            var order3 = asks.Dequeue();
            Assert.AreEqual(2000.0, order3.Price);
        }

        [Test]
        public void TestAsksOrderQueueDifferentExchanges()
        {
            var prices = new double[] { 1000.0, 2000.0, 1500.0 };
            var asks = new OrderPriorityQueue();

            foreach (var price in prices)
            {
                var exchangeInfo = new ExchangeInfo { BalanceBTC = 0.5, BalanceEUR = (price < 2000 && price > 1000) ? 0.0 : 500.0 };
                var order = new ExchangeOrder { Type = ExchangeOrder.OrderType.Sell, Amount = 1.0, Price = price, MyExchangeInfo = exchangeInfo };
                asks.Enqueue(order, order);
            }

            // buy orders should be returned in descending order, except orders with exchanges with insufficient balance - those are returned last
            var order1 = asks.Dequeue();
            Assert.AreEqual(1000.0, order1.Price);
            Assert.AreEqual(500.0, order1.MyExchangeInfo.BalanceEUR);

            var order2 = asks.Dequeue();
            Assert.AreEqual(2000.0, order2.Price);
            Assert.AreEqual(500.0, order2.MyExchangeInfo.BalanceEUR);

            var order3 = asks.Dequeue();
            Assert.AreEqual(1500.0, order3.Price);
            Assert.AreEqual(0.0, order3.MyExchangeInfo.BalanceEUR);
        }
    }
}