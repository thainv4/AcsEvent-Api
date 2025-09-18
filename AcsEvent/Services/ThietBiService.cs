using AcsEvent.Context;
using AcsEvent.DTOs.ThietBi;
using Microsoft.EntityFrameworkCore;

namespace AcsEvent.Services;

public class ThietBiService
{
    private readonly AcsEventDbContext _context;

    public ThietBiService(AcsEventDbContext context)
    {
        _context = context;
    }

    public async Task<ThietBiAuthorDto> GetThietBiAuthInfoAsync(int thietBiId)
    {
        try
        {
            var thietBi = await _context.ThietBis
                .Where(t => t.Id == thietBiId)
                .Select(t => new ThietBiAuthorDto
                {
                    IP = t.IP,
                    username = t.username,
                    password = t.password
                })
                .FirstOrDefaultAsync();

            if (thietBi == null)
            {
                return null;
            }

            return thietBi;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving ThietBi with ID {thietBiId}: {ex.Message}", ex);
        }
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

    public async Task<bool> ThietBiExistsAsync(int thietBiId)
    {
        return await _context.ThietBis.AnyAsync(t => t.Id == thietBiId);
    }
}