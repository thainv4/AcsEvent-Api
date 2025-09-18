using AcsEvent.DTOs.PhongBan;
using AcsEvent.DTOs.ThietBi;
using AcsEvent.Entities;
using AutoMapper;

namespace AcsEvent.Mapping;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        // ThietBi
        CreateMap<AddThietBiRequestDto, ThietBi>();
        CreateMap<ThietBiAuthorDto, ThietBi>();
        
        // PhongBan
        CreateMap<AddPhongBanRequestDto, PhongBan>();
    }
}