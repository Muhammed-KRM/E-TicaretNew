using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Enums;

namespace ETicaret.Business.Services;

/// <summary>
/// Fatura yönetimi ve PDF oluşturma servisi
/// QuestPDF kullanarak profesyonel faturalar oluşturur
/// </summary>
public class InvoiceManager : IInvoiceService
{
    private readonly AppDbContext _context;
    private readonly ILogger<InvoiceManager> _logger;
    private readonly ISettingService _settingService;
    private readonly string _invoiceDirectory;

    public InvoiceManager(
        AppDbContext context,
        ILogger<InvoiceManager> logger,
        ISettingService settingService)
    {
        _context = context;
        _logger = logger;
        _settingService = settingService;
        _invoiceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
        
        // Invoice klasörünü oluştur
        if (!Directory.Exists(_invoiceDirectory))
        {
            Directory.CreateDirectory(_invoiceDirectory);
        }

        // QuestPDF lisans ayarı (Community license)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<GenerateInvoiceResult> GenerateInvoiceAsync(Guid orderId)
    {
        _logger.LogInformation("Generating invoice for order {OrderId}", orderId);
        
        try
        {
            // Fatura verisini hazırla
            var invoiceData = await PrepareInvoiceDataAsync(orderId);
            
            // Fatura numarası oluştur
            var invoiceNumber = await GenerateInvoiceNumberAsync();
            invoiceData.InvoiceNumber = invoiceNumber;

            // PDF oluştur
            var pdfBytes = await Task.Run(() => GenerateInvoicePdf(invoiceData));
            
            // Dosyaya kaydet
            var fileName = $"{invoiceNumber}.pdf";
            var filePath = Path.Combine(_invoiceDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            _logger.LogInformation("Invoice generated successfully: {InvoiceNumber}", invoiceNumber);

            return new GenerateInvoiceResult
            {
                Success = true,
                Message = "Fatura başarıyla oluşturuldu",
                InvoiceNumber = invoiceNumber,
                InvoiceUrl = $"/invoices/{fileName}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for order {OrderId}", orderId);
            return new GenerateInvoiceResult
            {
                Success = false,
                Message = "Fatura oluşturulurken bir hata oluştu"
            };
        }
    }

    public async Task<InvoiceDto> PrepareInvoiceDataAsync(Guid orderId)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new Exception("Sipariş bulunamadı");
        }

        // Firma bilgilerini al
        var companyInfo = await GetCompanyInfoAsync();

        // Adres bilgilerini ShippingAddressSnapshot'tan parse et
        var addressParts = order.ShippingAddressSnapshot.Split(',');
        
        // Müşteri bilgilerini hazırla
        var customerInfo = new CustomerInfoDto
        {
            Name = order.User?.FullName ?? order.GuestName ?? "Misafir Müşteri",
            Email = order.User?.Email ?? order.GuestEmail ?? "",
            Phone = order.GuestPhone ?? "",
            Address = order.ShippingAddressSnapshot,
            City = addressParts.Length > 1 ? addressParts[^2].Trim() : "",
            District = addressParts.Length > 2 ? addressParts[^3].Trim() : "",
            PostalCode = ""
        };

        // Fatura kalemlerini hazırla
        var items = order.Items.Select(oi => new InvoiceItemDto
        {
            ProductName = oi.Product.Title,
            ProductCode = oi.Product.SKU ?? oi.ProductId.ToString().Substring(0, 8),
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,
            TotalPrice = oi.TotalPrice,
            TaxRate = 20.00m, // KDV %20
            TaxAmount = oi.TotalPrice * 0.20m
        }).ToList();

        var invoice = new InvoiceDto
        {
            InvoiceNumber = "", // GenerateInvoiceAsync'de set edilecek
            OrderNumber = order.OrderNumber,
            InvoiceDate = DateTime.Now,
            CompanyInfo = companyInfo,
            CustomerInfo = customerInfo,
            Items = items,
            SubTotal = order.SubTotal,
            TaxAmount = order.SubTotal * 0.20m, // KDV
            ShippingCost = order.ShippingPrice,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalPrice,
            PaymentMethod = order.PaymentStatus == PaymentStatus.Paid ? "Kredi Kartı" : "Havale",
            Notes = order.CustomerNote
        };

        return invoice;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid orderId)
    {
        var invoiceData = await PrepareInvoiceDataAsync(orderId);
        invoiceData.InvoiceNumber = await GenerateInvoiceNumberAsync();
        
        return await Task.Run(() => GenerateInvoicePdf(invoiceData));
    }

    private byte[] GenerateInvoicePdf(InvoiceDto invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);

                void ComposeHeader(IContainer container)
                {
                    container.Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(invoice.CompanyInfo.Name)
                                .FontSize(20).Bold().FontColor(Colors.Green.Darken2);
                            
                            column.Item().Text(invoice.CompanyInfo.Address).FontSize(9);
                            column.Item().Text($"Vergi Dairesi: {invoice.CompanyInfo.TaxOffice}").FontSize(9);
                            column.Item().Text($"Vergi No: {invoice.CompanyInfo.TaxNumber}").FontSize(9);
                            column.Item().Text($"Tel: {invoice.CompanyInfo.Phone}").FontSize(9);
                            column.Item().Text($"Email: {invoice.CompanyInfo.Email}").FontSize(9);
                        });

                        row.ConstantItem(150).Column(column =>
                        {
                            column.Item().AlignRight().Text("FATURA").FontSize(18).Bold();
                            column.Item().AlignRight().Text($"No: {invoice.InvoiceNumber}").FontSize(10);
                            column.Item().AlignRight().Text($"Tarih: {invoice.InvoiceDate:dd.MM.yyyy}").FontSize(10);
                            column.Item().AlignRight().Text($"Sipariş: {invoice.OrderNumber}").FontSize(9);
                        });
                    });
                }

                void ComposeContent(IContainer container)
                {
                    container.PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(10);

                        // Müşteri Bilgileri
                        column.Item().Element(ComposeCustomerInfo);

                        // Fatura Kalemleri
                        column.Item().Element(ComposeTable);

                        // Toplam Bilgileri
                        column.Item().Element(ComposeTotals);

                        // Notlar
                        if (!string.IsNullOrEmpty(invoice.Notes))
                        {
                            column.Item().PaddingTop(10).Text($"Not: {invoice.Notes}")
                                .FontSize(9).Italic();
                        }
                    });
                }

                void ComposeCustomerInfo(IContainer container)
                {
                    container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
                    {
                        column.Item().Text("FATURA EDİLEN").FontSize(11).Bold();
                        column.Item().Text(invoice.CustomerInfo.Name);
                        column.Item().Text($"{invoice.CustomerInfo.Address}").FontSize(9);
                        column.Item().Text($"{invoice.CustomerInfo.District}, {invoice.CustomerInfo.City}").FontSize(9);
                        column.Item().Text($"Tel: {invoice.CustomerInfo.Phone}").FontSize(9);
                        column.Item().Text($"Email: {invoice.CustomerInfo.Email}").FontSize(9);
                    });
                }

                void ComposeTable(IContainer container)
                {
                    container.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // #
                            columns.RelativeColumn(3);   // Ürün Adı
                            columns.ConstantColumn(60);  // Adet
                            columns.ConstantColumn(80);  // Birim Fiyat
                            columns.ConstantColumn(80);  // Toplam
                            columns.ConstantColumn(60);  // KDV
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#");
                            header.Cell().Element(CellStyle).Text("Ürün Adı");
                            header.Cell().Element(CellStyle).AlignRight().Text("Adet");
                            header.Cell().Element(CellStyle).AlignRight().Text("Birim Fiyat");
                            header.Cell().Element(CellStyle).AlignRight().Text("Toplam");
                            header.Cell().Element(CellStyle).AlignRight().Text("KDV %");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.Background(Colors.Green.Darken2)
                                    .Padding(5).DefaultTextStyle(x => x.FontColor(Colors.White).Bold());
                            }
                        });

                        // Items
                        int index = 1;
                        foreach (var item in invoice.Items)
                        {
                            table.Cell().Element(CellStyle).Text($"{index++}");
                            table.Cell().Element(CellStyle).Text(item.ProductName);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:N2} ₺");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalPrice:N2} ₺");
                            table.Cell().Element(CellStyle).AlignRight().Text($"%{item.TaxRate:N0}");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                            }
                        }
                    });
                }

                void ComposeTotals(IContainer container)
                {
                    container.AlignRight().Column(column =>
                    {
                        column.Spacing(5);

                        column.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Ara Toplam:");
                            row.ConstantItem(100).AlignRight().Text($"{invoice.SubTotal:N2} ₺");
                        });

                        if (invoice.DiscountAmount > 0)
                        {
                            column.Item().Row(row =>
                            {
                                row.ConstantItem(120).Text("İndirim:");
                                row.ConstantItem(100).AlignRight().Text($"-{invoice.DiscountAmount:N2} ₺")
                                    .FontColor(Colors.Red.Medium);
                            });
                        }

                        column.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("KDV (%20):");
                            row.ConstantItem(100).AlignRight().Text($"{invoice.TaxAmount:N2} ₺");
                        });

                        column.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Kargo:");
                            row.ConstantItem(100).AlignRight().Text($"{invoice.ShippingCost:N2} ₺");
                        });

                        column.Item().PaddingTop(10).Row(row =>
                        {
                            row.ConstantItem(120).Text("GENEL TOPLAM:").FontSize(12).Bold();
                            row.ConstantItem(100).AlignRight().Text($"{invoice.TotalAmount:N2} ₺")
                                .FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                        });
                    });
                }

                void ComposeFooter(IContainer container)
                {
                    container.AlignCenter().Column(column =>
                    {
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        column.Item().PaddingTop(5).Text("Bu fatura elektronik ortamda oluşturulmuştur.")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                        column.Item().Text(invoice.CompanyInfo.Website ?? "www.ozelders.com")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                }
            });
        });

        return document.GeneratePdf();
    }

    public async Task<PagedResult<PaymentHistoryDto>> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId && o.PaymentStatus == PaymentStatus.Paid)
                .OrderByDescending(o => o.PaidAt);

            var totalCount = await query.CountAsync();

            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new PaymentHistoryDto
                {
                    Id = o.Id,
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    PaymentDate = o.PaidAt ?? o.CreatedAt,
                    Amount = o.TotalPrice,
                    PaymentMethod = o.PaymentStatus == PaymentStatus.Paid ? "Kredi Kartı" : "Havale",
                    Status = o.PaymentStatus.ToString(),
                    InvoiceNumber = null, // TODO: Invoice tracking eklenecek
                    CanDownloadInvoice = o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped
                })
                .ToListAsync();

            return new PagedResult<PaymentHistoryDto>
            {
                Items = payments,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        // TODO: Invoice numarası ile sipariş eşleştirme yapılacak
        // Şimdilik placeholder
        await Task.CompletedTask;
        return null;
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var now = DateTime.Now;
        var prefix = $"{now:yyyy-MM}";
        
        // Bu ay oluşturulan fatura sayısını bul
        var count = await _context.Orders
            .CountAsync(o => o.CreatedAt.Month == now.Month && o.CreatedAt.Year == now.Year);
        
        var sequence = (count + 1).ToString("D5");
        
        return $"{prefix}-{sequence}";
    }

    public async Task<CompanyInfoDto> GetCompanyInfoAsync()
    {
        // Setting service'den firma bilgilerini al
        // Şimdilik hardcoded, sonra GlobalSettings'den alınacak
        await Task.CompletedTask;
        
        return new CompanyInfoDto
        {
            Name = "OzelDers E-Ticaret",
            Address = "Örnek Mahallesi, Örnek Sokak No:1, İstanbul",
            TaxOffice = "Kadıköy",
            TaxNumber = "1234567890",
            Phone = "+90 555 123 45 67",
            Email = "info@ozelders.com",
            Website = "www.ozelders.com"
        };
    }

    public async Task<bool> UpdateCompanyInfoAsync(CompanyInfoDto companyInfo)
    {
        // TODO: GlobalSettings'e kaydet
        await Task.CompletedTask;
        return true;
    }

    public async Task<string?> GetInvoiceNumberByOrderIdAsync(Guid orderId)
    {
        // TODO: Order entity'de invoice number field eklenecek
        await Task.CompletedTask;
        return null;
    }

    public async Task<string?> GetInvoiceFilePathAsync(string invoiceNumber)
    {
        var fileName = $"{invoiceNumber}.pdf";
        var filePath = Path.Combine(_invoiceDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            return await Task.FromResult(filePath);
        }
        
        return null;
    }
}
