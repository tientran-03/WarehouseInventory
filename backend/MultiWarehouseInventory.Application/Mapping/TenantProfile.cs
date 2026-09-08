using AutoMapper;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Application.Mapping
{
    public class TenantProfile : Profile
    {
        public TenantProfile()
        {
            CreateMap<Tenant, TenantResponse>()
                .ForCtorParam("Accounts", opt => opt.MapFrom(_ => Array.Empty<CompanyAccountResponse>()));
            CreateMap<UpsertTenantRequest, Tenant>();
        }
    }
}
