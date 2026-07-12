using ETicaret.Business.Events;
using ETicaret.Business.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ETicaret.Worker.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IEmailService _emailService;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Sipariş alındı. OrderId={OrderId}, OrderNumber={OrderNumber}", msg.OrderId, msg.OrderNumber);

        if (!string.IsNullOrEmpty(msg.UserEmail))
        {
            await _emailService.SendEmailAsync(
                msg.UserEmail, 
                $"Siparişiniz Alındı - {msg.OrderNumber}", 
                $"<p>Siparişiniz başarıyla alınmıştır. Toplam tutar: {msg.TotalPrice:C2}</p>"
            );
        }
    }
}
