using System;
using AutoMapper;
using Contracts;
using SearchService_controllers.Models;

namespace SearchService_controllers.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
        CreateMap<AuctionUpdated, Item>();
    }
}
