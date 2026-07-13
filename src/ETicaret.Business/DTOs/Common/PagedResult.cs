namespace ETicaret.Business.DTOs;

/// <summary>
/// Sayfalanmış veri listesi için generic wrapper DTO
/// </summary>
/// <typeparam name="T">Liste elemanlarının tipi</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Veri listesi
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// Toplam kayıt sayısı
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Mevcut sayfa numarası (1-based)
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Sayfa başına kayıt sayısı
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Toplam sayfa sayısı
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Bir önceki sayfa var mı?
    /// </summary>
    public bool HasPrevious => Page > 1;
    
    /// <summary>
    /// Bir sonraki sayfa var mı?
    /// </summary>
    public bool HasNext => Page < TotalPages;
}
