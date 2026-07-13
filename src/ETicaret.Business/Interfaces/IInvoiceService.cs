using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

/// <summary>
/// Fatura işlemleri için servis interface'i
/// QuestPDF kullanarak PDF oluşturur
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Sipariş için fatura oluşturur ve PDF olarak kaydeder
    /// Fatura numarası otomatik generate edilir (YYYY-MM-XXXXX formatında)
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <returns>Fatura oluşturma sonucu (dosya yolu/URL ile)</returns>
    Task<GenerateInvoiceResult> GenerateInvoiceAsync(Guid orderId);
    
    /// <summary>
    /// Sipariş için fatura DTO'sunu hazırlar (PDF oluşturmadan)
    /// Önizleme veya veri hazırlığı için kullanılır
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <returns>Fatura DTO'su</returns>
    Task<InvoiceDto> PrepareInvoiceDataAsync(Guid orderId);
    
    /// <summary>
    /// Fatura PDF'ini byte array olarak döner
    /// İndirme endpoint'lerinde kullanılır
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <returns>PDF byte array</returns>
    Task<byte[]> GenerateInvoicePdfAsync(Guid orderId);
    
    /// <summary>
    /// Kullanıcının ödeme geçmişini getirir
    /// Fatura indirme linkleri dahil
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt</param>
    /// <returns>Sayfalanmış ödeme geçmişi</returns>
    Task<PagedResult<PaymentHistoryDto>> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Belirli bir faturayı numarası ile getirir
    /// </summary>
    /// <param name="invoiceNumber">Fatura numarası</param>
    /// <returns>Fatura DTO'su veya null</returns>
    Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber);
    
    /// <summary>
    /// Fatura numarası oluşturur
    /// Format: YYYY-MM-XXXXX (örn: 2025-01-00001)
    /// </summary>
    /// <returns>Benzersiz fatura numarası</returns>
    Task<string> GenerateInvoiceNumberAsync();
    
    /// <summary>
    /// Firma bilgilerini getirir (ayarlardan)
    /// Fatura üzerinde gösterilecek firma bilgileri
    /// </summary>
    /// <returns>Firma bilgileri DTO'su</returns>
    Task<CompanyInfoDto> GetCompanyInfoAsync();
    
    /// <summary>
    /// Firma bilgilerini günceller (Admin)
    /// </summary>
    /// <param name="companyInfo">Yeni firma bilgileri</param>
    /// <returns>İşlem başarılı mı</returns>
    Task<bool> UpdateCompanyInfoAsync(CompanyInfoDto companyInfo);
    
    /// <summary>
    /// Sipariş için fatura oluşturulmuş mu kontrol eder
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <returns>Fatura numarası varsa döner, yoksa null</returns>
    Task<string?> GetInvoiceNumberByOrderIdAsync(Guid orderId);
    
    /// <summary>
    /// Fatura PDF dosyasının sunucudaki yolunu döner
    /// </summary>
    /// <param name="invoiceNumber">Fatura numarası</param>
    /// <returns>Dosya yolu veya null</returns>
    Task<string?> GetInvoiceFilePathAsync(string invoiceNumber);
}
