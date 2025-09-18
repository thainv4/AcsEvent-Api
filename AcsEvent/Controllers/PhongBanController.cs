using AcsEvent.Context;
using AcsEvent.DTOs.PhongBan;
using AcsEvent.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcsEvent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhongBanController : ControllerBase
{
    private readonly AcsEventDbContext _context;
    private readonly IMapper _mapper;
    public PhongBanController(AcsEventDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpPost]
    public IActionResult AddPhongBan([FromBody] AddPhongBanRequestDto phongBanDto)
    {
        try
        {
            var phongBan = _mapper.Map<PhongBan>(phongBanDto);
            _context.PhongBans.Add(phongBan);
            _context.SaveChanges();
            return Ok(new
            {
                message = "Thêm phòng ban thành công",
                id = phongBan.MaPb,
                tenPb = phongBan.TenPb
            });
        }
        catch (Exception e)
        {
            var innerException = e.InnerException?.Message ?? e.Message;
            return BadRequest(new
            {
                message = "Lỗi khi lưu dữ liệu",
                error = e.Message,
                innerError = innerException
            });
        }
    }

    [HttpGet]
    public IActionResult GetAllPhongBans()
    {
        try
        {
            var phongBans = _context.PhongBans.ToList();
            return Ok(new
            {
                message = "Lấy danh sách phòng ban thành công",
                data = phongBans
            });
        }
        catch (Exception e)
        {
            var innerException = e.InnerException?.Message ?? e.Message;
            return BadRequest(new
            {
                message = "Lỗi khi lấy dữ liệu",
                error = e.Message,
                innerError = innerException
            });
        }
    }
}