using ETicaret.Business.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ETicaret.Worker.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        _logger.LogInformation("Yeni ürün eklendi: ProductId={ProductId}. İlgili işlemler (Elasticsearch indexleme vb.) burada yapılabilir.", context.Message.ProductId);
        return Task.CompletedTask;
    }
}
