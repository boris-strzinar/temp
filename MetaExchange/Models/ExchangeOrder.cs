using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MetaExchange.Models
{
    public class ExchangeOrder
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
}
