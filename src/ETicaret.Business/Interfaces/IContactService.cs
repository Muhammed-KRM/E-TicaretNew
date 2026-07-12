using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface IContactService
{
    // Herkese açık
    Task<ContactInfoDto> GetContactInfoAsync();
    Task SendMessageAsync(ContactMessageCreateDto dto);

    // Admin
    Task UpdateContactInfoAsync(ContactInfoDto dto);
    Task<List<ContactMessageDto>> GetMessagesAsync(bool? isRead = null, int page = 1, int pageSize = 20);
    Task MarkAsReadAsync(int messageId);
    Task ReplyToMessageAsync(int messageId, string reply);
}
