using AcsEvent.Context;
using AcsEvent.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcsEvent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly AcsEventDbContext _context;
    
    public EmployeeController(AcsEventDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult AddEmployeeInfo(EmployeeInfo? employeeInfo)
    {
        try
        {
            _context.EmployeeInfos.Add(employeeInfo);
            _context.SaveChanges();
            return Ok(new
            {
                message = "Thêm thông tin nhân viên thành công",
                maCC = employeeInfo.MaCC,
                hoTen = employeeInfo.HoTen,
                maNV = employeeInfo.MaNV,
                maPb = employeeInfo.MaPb
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
}