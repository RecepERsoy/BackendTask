using MassTransit;
using Serilog;
using Product.Application.Events;

namespace Log.Api.Consumers
{
    public class ProductAddedEventConsumer : IConsumer<ProductAddedEvent>
    {
        public Task Consume(ConsumeContext<ProductAddedEvent> context)
        {
            var message = context.Message;

            Serilog.Log.Information("Yeni ürün eklendi. {@ProductData}", new
            {
                UrunId = message.Id,
                UrunAdi = message.Name,
                EklemeZamani = DateTime.UtcNow
            });

            return Task.CompletedTask;
        }
    }
}