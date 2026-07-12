namespace ETicaret.Business.Events;

public class ProductCreatedEvent
{
    public Guid ProductId { get; set; }
}

public class ProductUpdatedEvent
{
    public Guid ProductId { get; set; }
}

public class ProductDeletedEvent
{
    public Guid ProductId { get; set; }
}
