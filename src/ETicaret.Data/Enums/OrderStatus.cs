namespace ETicaret.Data.Enums;

/// <summary>
/// Siparişin durumunu belirtir.
/// </summary>
public enum OrderStatus
{
    /// <summary>Ödeme bekleniyor.</summary>
    Pending = 0,
    
    /// <summary>Ödeme alındı, onaylandı.</summary>
    Paid = 1,
    
    /// <summary>Sipariş hazırlanıyor.</summary>
    Preparing = 2,
    
    /// <summary>Kargoya verildi.</summary>
    Shipped = 3,
    
    /// <summary>Teslim edildi.</summary>
    Delivered = 4,
    
    /// <summary>İptal edildi.</summary>
    Cancelled = 5,
    
    /// <summary>İade edildi / Ücret iadesi yapıldı.</summary>
    Refunded = 6,
    
    /// <summary>İade talep edildi.</summary>
    ReturnRequested = 7,
    
    /// <summary>İade talebi reddedildi.</summary>
    ReturnRejected = 8,
    
    /// <summary>Kısmen iade edildi.</summary>
    PartiallyRefunded = 9,
    
    /// <summary>Ödeme başarısız.</summary>
    Failed = 10
}

