using AutoMapper;
using Sale.Database.Entities;
using Sale.Model.Base;

namespace Sale.ServiceLayer.MappingProfiles
{
    internal class BasicProfile : Profile
    {
        public BasicProfile() {
            CreateMap<ApplicationUserModel, ApplicationUser>().ReverseMap();
        }
    }
}
