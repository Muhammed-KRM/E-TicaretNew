namespace ETicaret.Business.Exceptions;

public class OutOfStockException : BusinessException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }

    public OutOfStockException(Guid productId, int requestedQuantity) 
        : base($"Ürün stokta yetersiz. İstenen: {requestedQuantity}")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
    }
}
