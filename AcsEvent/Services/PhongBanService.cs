using AcsEvent.Context;
using AcsEvent.Entities;
using AcsEvent.Interface;

namespace AcsEvent.Services;

public class PhongBanService : IPhongBanService
{
    private readonly AcsEventDbContext _context;
    
    public PhongBanService(AcsEventDbContext context)
    {
        _context = context;
    }
    
    public Task<List<PhongBan>> GetPhongBansAsync()
    {
        return Task.FromResult(_context.PhongBans.ToList());
    }
}