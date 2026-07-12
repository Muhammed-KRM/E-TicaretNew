namespace ETicaret.Business.Helpers;

public static class OrderNumberGenerator
{
    public static string Generate()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
    }
}
