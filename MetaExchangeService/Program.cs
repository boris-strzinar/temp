using MetaExchange;

namespace MetaExchangeService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var orderBookDataFilePath = builder.Configuration.GetValue<string>("OrderBookDataFile");
            var orderBookDataLimit = builder.Configuration.GetValue<long>("OrderBookDataLimit");
            builder.Services.AddSingleton<IOrderBookService, OrderBookService>(service => new OrderBookService(orderBookDataFilePath, orderBookDataLimit));


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}