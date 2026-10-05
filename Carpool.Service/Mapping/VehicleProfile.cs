using AutoMapper;
using Carpool.Core.Dtos.Vehicles;
using Carpool.Core.Entities;

namespace Carpool.Service.Mapping;

/// <summary>
/// AutoMapper profile for <see cref="Vehicle"/> (spec §34). On the request → entity maps
/// the key and owner are never taken from the client — <c>OwnerId</c> is set from the JWT
/// in the Service layer (spec §43).
/// </summary>
public class VehicleProfile : Profile
{
    public VehicleProfile()
    {
        CreateMap<Vehicle, VehicleResponse>();

        CreateMap<CreateVehicleRequest, Vehicle>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OwnerId, o => o.Ignore())
            .ForMember(d => d.Owner, o => o.Ignore())
            .ForMember(d => d.Rides, o => o.Ignore());

        // Applied onto an already-loaded, tracked Vehicle in VehicleService.UpdateAsync.
        CreateMap<UpdateVehicleRequest, Vehicle>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OwnerId, o => o.Ignore())
            .ForMember(d => d.Owner, o => o.Ignore())
            .ForMember(d => d.Rides, o => o.Ignore());
    }
}
