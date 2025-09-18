using AcsEvent.Context;
using AcsEvent.DTOs.ThietBi;
using AcsEvent.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcsEvent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ThietBiController : ControllerBase
{
    private readonly AcsEventDbContext _context;
    private readonly IMapper _mapper;

    public ThietBiController(AcsEventDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    [HttpPost]
    public IActionResult AddThietBi([FromBody] AddThietBiRequestDto thietBiDto)
    {
        try
        {
            var thietBi = _mapper.Map<ThietBi>(thietBiDto);
            _context.ThietBis.Add(thietBi);
            _context.SaveChanges();
        
            return Ok(new { 
                message = "Thêm thiết bị thành công", 
                id = thietBi.Id ,
                tenTB = thietBi.TenTB
            });
        }
        catch (Exception ex)
        {
            // Xem chi tiết inner exception
            var innerException = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new { 
                message = "Lỗi khi lưu dữ liệu", 
                error = ex.Message,
                innerError = innerException
            });
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetThietBiById(int id)
    {
        var thietBi = _context.ThietBis.Find(id);
        if (thietBi == null)
            return NotFound();
        return Ok(thietBi);
    }

    [HttpGet]
    public IActionResult GetAllThietBis()
    {
        var thietBis = _context.ThietBis.ToList();
        return Ok(thietBis);
    }
}