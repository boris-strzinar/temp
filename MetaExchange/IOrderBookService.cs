using MetaExchange.Models;

namespace MetaExchange
{
    public interface IOrderBookService
    {
        public string? ReadOrderBookDataFile(string path, long limit = 0);
        public (double, IEnumerable<ExchangeOrder>) Buy(double amount);
        public (double, IEnumerable<ExchangeOrder>) Sell(double amount);
    }
}
