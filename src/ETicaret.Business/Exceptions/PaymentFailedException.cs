namespace ETicaret.Business.Exceptions;

public class PaymentFailedException : BusinessException
{
    public string Reason { get; }

    public PaymentFailedException(string reason) 
        : base($"Ödeme başarısız: {reason}")
    {
        Reason = reason;
    }
}
