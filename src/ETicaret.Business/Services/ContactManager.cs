using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Business.Services;

public class ContactManager : IContactService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public ContactManager(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<ContactInfoDto> GetContactInfoAsync()
    {
        var info = await _db.ContactInfo.FirstOrDefaultAsync();
        if (info == null)
        {
            return new ContactInfoDto(); // Boş döner
        }

        return new ContactInfoDto
        {
            CompanyName = info.CompanyName,
            Address = info.Address,
            Phone = info.Phone,
            Email = info.Email,
            WorkingHours = info.WorkingHours,
            Description = info.Description,
            MapEmbedUrl = info.MapEmbedUrl,
            InstagramUrl = info.InstagramUrl,
            FacebookUrl = info.FacebookUrl,
            TwitterUrl = info.TwitterUrl
        };
    }

    public async Task SendMessageAsync(ContactMessageCreateDto dto)
    {
        var message = new ContactMessage
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Subject = dto.Subject,
            Message = dto.Message,
            IsRead = false,
            IsReplied = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateContactInfoAsync(ContactInfoDto dto)
    {
        var info = await _db.ContactInfo.FirstOrDefaultAsync();
        if (info == null)
        {
            info = new ContactInfo();
            _db.ContactInfo.Add(info);
        }

        info.CompanyName = dto.CompanyName;
        info.Address = dto.Address;
        info.Phone = dto.Phone;
        info.Email = dto.Email;
        info.WorkingHours = dto.WorkingHours;
        info.Description = dto.Description;
        info.MapEmbedUrl = dto.MapEmbedUrl;
        info.InstagramUrl = dto.InstagramUrl;
        info.FacebookUrl = dto.FacebookUrl;
        info.TwitterUrl = dto.TwitterUrl;
        info.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<ContactMessageDto>> GetMessagesAsync(bool? isRead = null, int page = 1, int pageSize = 20)
    {
        var query = _db.ContactMessages.AsQueryable();

        if (isRead.HasValue)
        {
            query = query.Where(m => m.IsRead == isRead.Value);
        }

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(m => new ContactMessageDto
        {
            Id = m.Id,
            FullName = m.FullName,
            Email = m.Email,
            Phone = m.Phone,
            Subject = m.Subject,
            Message = m.Message,
            IsRead = m.IsRead,
            IsReplied = m.IsReplied,
            AdminReply = m.AdminReply,
            CreatedAt = m.CreatedAt,
            RepliedAt = m.RepliedAt
        }).ToList();
    }

    public async Task MarkAsReadAsync(int messageId)
    {
        var message = await _db.ContactMessages.FindAsync(messageId);
        if (message != null && !message.IsRead)
        {
            message.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task ReplyToMessageAsync(int messageId, string reply)
    {
        var message = await _db.ContactMessages.FindAsync(messageId);
        if (message == null) throw new Exception("Mesaj bulunamadı.");

        message.AdminReply = reply;
        message.IsReplied = true;
        message.RepliedAt = DateTime.UtcNow;
        message.IsRead = true; // Yanıtlandıysa okunmuştur.

        await _db.SaveChangesAsync();

        // Kullanıcıya e-posta gönder
        await _emailService.SendEmailAsync(
            message.Email,
            $"RE: {message.Subject}",
            $"Sayın {message.FullName},<br><br>Mesajınız:<br><i>{message.Message}</i><br><br>Yanıtımız:<br>{reply}"
        );
    }
}
