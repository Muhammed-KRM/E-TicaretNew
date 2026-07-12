namespace ETicaret.Data.Enums;

/// <summary>
/// İndirim türünü belirtir.
/// </summary>
public enum DiscountType
{
    /// <summary>Yüzdelik indirim (ör: %20).</summary>
    Percentage = 0,
    
    /// <summary>Sabit tutar indirimi (ör: 50 TL).</summary>
    FixedAmount = 1
}

