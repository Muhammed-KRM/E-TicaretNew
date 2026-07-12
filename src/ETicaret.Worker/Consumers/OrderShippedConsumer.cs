using ETicaret.Business.Events;
using ETicaret.Business.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ETicaret.Worker.Consumers;

public class OrderShippedConsumer : IConsumer<OrderShippedEvent>
{
    private readonly ILogger<OrderShippedConsumer> _logger;
    private readonly IEmailService _emailService;

    public OrderShippedConsumer(ILogger<OrderShippedConsumer> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Kargo bildirimi gönderiliyor. OrderId={OrderId}, OrderNumber={OrderNumber}", msg.OrderId, msg.OrderNumber);

        if (!string.IsNullOrEmpty(msg.UserEmail))
        {
            await _emailService.SendEmailAsync(
                msg.UserEmail, 
                $"Siparişiniz Kargoya Verildi - {msg.OrderNumber}", 
                $"<p>Siparişiniz kargoya verilmiştir. Kargo Takip No: <strong>{msg.TrackingNumber}</strong></p>"
            );
        }
    }
}
