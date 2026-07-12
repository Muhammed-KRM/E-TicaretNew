using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    // Herkese açık: İletişim bilgilerini getir
    [HttpGet("info")]
    [AllowAnonymous]
    public async Task<ActionResult<ContactInfoDto>> GetContactInfo()
    {
        var info = await _contactService.GetContactInfoAsync();
        return Ok(info);
    }

    // Herkese açık: İletişim formu gönder
    [HttpPost("message")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage(ContactMessageCreateDto dto)
    {
        await _contactService.SendMessageAsync(dto);
        return Ok(new { message = "Mesajınız başarıyla gönderildi." });
    }
}
