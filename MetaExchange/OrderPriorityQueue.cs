using MetaExchange.Models;

namespace MetaExchange
{
    public class OrderPriorityQueue : PriorityQueue<ExchangeOrder, ExchangeOrder>
    {
        class OrderComparer : IComparer<ExchangeOrder>
        {
            public int Compare(ExchangeOrder? o1, ExchangeOrder? o2)
            {
                // first check balance constraints
                bool orderPossible1, orderPossible2;
                if (o1?.Type == ExchangeOrder.OrderType.Sell)
                {
                    // the order is selling, we're buying so we need to have proper EUR balance
                    orderPossible1 = o1?.MyExchangeInfo?.BalanceEUR > 0.0;
                    orderPossible2 = o2?.MyExchangeInfo?.BalanceEUR > 0.0;
                }
                else
                {
                    // the order is buying, we're selling so we need to have proper BTC balance
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
                    return o1.Type == ExchangeOrder.OrderType.Buy ? 1 : -1;
                }
                else if (o1.Price > o2.Price)
                {
                    return o1.Type == ExchangeOrder.OrderType.Buy ? -1 : 1;
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
                ExchangeOrder order = o.Order;
                order.MyExchangeInfo = exchangeInfo;
                Enqueue(order, order);
            }
        }
    }
}
