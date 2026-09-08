using AutoMapper;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponse>();
        CreateMap<UpsertProductRequest, Product>();
    }
}
