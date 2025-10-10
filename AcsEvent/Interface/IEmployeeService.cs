using AcsEvent.Entities;

namespace AcsEvent.Interface;

public interface IEmployeeService
{
    Task<List<EmployeeInfo?>> GetEmployeesByPhongBanIdAsync(int phongBanId);   
}