using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETicaret.Business.DTOs;
using ETicaret.Business.Interfaces;

namespace ETicaret.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpPost("validate")]
    public async Task<ActionResult<CouponResultDto>> ValidateCoupon(CouponValidateDto dto)
    {
        var result = await _couponService.ValidateCouponAsync(dto.Code, dto.CartTotal);
        return Ok(result);
    }
}

public record CouponValidateDto(string Code, decimal CartTotal);
