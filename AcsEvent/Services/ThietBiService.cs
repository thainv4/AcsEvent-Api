using System.Text;
using AcsEvent.Context;
using AcsEvent.DTOs.ThietBi;
using AcsEvent.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcsEvent.Services;

public class ThietBiService
{
    private readonly AcsEventDbContext _context;

    public ThietBiService(AcsEventDbContext context)
    {
        _context = context;
    }

    public async Task<List<ThietBiAuthorDto>> GetThietBiAuthInfosAsync()
    {
        try
        {
            var thietBis = await _context.ThietBis.Select(t => new ThietBiAuthorDto
            {
                IP = t.IP,
                username = t.username,
                password = t.password
            }).ToListAsync();

            if (thietBis == null || thietBis.Count == 0)
            {
                return null;
            }

            return thietBis;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving ThietBi: {ex.Message}", ex);
        }
    }
}