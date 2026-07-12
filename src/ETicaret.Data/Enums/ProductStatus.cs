namespace ETicaret.Data.Enums;

/// <summary>
/// Ürünün mevcut yayın durumunu tanımlar.
/// </summary>
public enum ProductStatus
{
    /// <summary>Taslak — henüz yayınlanmamış, sadece satıcı görebilir.</summary>
    Draft = 0,

    /// <summary>Aktif — alıcılar tarafından görüntülenebilir ve satın alınabilir.</summary>
    Active = 1,

    /// <summary>Stok Tükendi — görüntülenebilir ama satın alınamaz.</summary>
    OutOfStock = 2,

    /// <summary>Pasif — satıcı tarafından geçici olarak gizlenmiş.</summary>
    Inactive = 3,

    /// <summary>Moderasyon Bekliyor — admin onayı bekleniyor.</summary>
    PendingReview = 4
}

