namespace MetaExchange.Models
{
    public class ExchangeOrderBook
    {
        public DateTime AcqTime { get; set; }
        public List<OrderEnvelope> Asks { get; set; }
        public List<OrderEnvelope> Bids { get; set; }
    }
}
