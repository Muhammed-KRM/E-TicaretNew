using ETicaret.Business.Events;
using ETicaret.Business.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ETicaret.Worker.Consumers;

public class RefundApprovedConsumer : IConsumer<RefundApprovedEvent>
{
    private readonly ILogger<RefundApprovedConsumer> _logger;
    private readonly IEmailService _emailService;

    public RefundApprovedConsumer(ILogger<RefundApprovedConsumer> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<RefundApprovedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("İade onay bildirimi gönderiliyor. OrderId={OrderId}, OrderNumber={OrderNumber}", msg.OrderId, msg.OrderNumber);

        if (!string.IsNullOrEmpty(msg.UserEmail))
        {
            await _emailService.SendEmailAsync(
                msg.UserEmail, 
                $"İade Talebiniz Onaylandı - {msg.OrderNumber}", 
                $"<p>Siparişinizdeki iade talebi onaylanmıştır. İade edilen tutar: <strong>{msg.RefundAmount:C2}</strong></p>"
            );
        }
    }
}
