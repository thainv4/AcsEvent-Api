using AcsEvent.Context;
using AcsEvent.Entities;
using AcsEvent.Interface;
using Microsoft.EntityFrameworkCore;

namespace AcsEvent.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AcsEventDbContext _context;
    
    public EmployeeService(AcsEventDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeInfo?>> GetEmployeesByPhongBanIdAsync(int phongBanId)
    {
        if (phongBanId == 0)
        {
            return await Task.FromResult<List<EmployeeInfo?>>(null);
        }
        
        return await Task.FromResult(_context.EmployeeInfos.Where(x => x.MaPb == phongBanId).ToList());
    }
    
}