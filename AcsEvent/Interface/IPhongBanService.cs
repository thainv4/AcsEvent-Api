using AcsEvent.Entities;

namespace AcsEvent.Interface;

public interface IPhongBanService
{
    Task<List<PhongBan>> GetPhongBansAsync(); 
} 