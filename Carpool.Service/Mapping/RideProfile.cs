using AutoMapper;
using Carpool.Core.Dtos.Rides;
using Carpool.Core.Entities;

namespace Carpool.Service.Mapping;

/// <summary>
/// AutoMapper profile for <see cref="Ride"/> (spec §34).
///
/// <see cref="Ride"/> → <see cref="RideResponse"/> flattens the driver and vehicle into
/// small info objects and projects the <c>RideTags</c> join rows to a plain tag list —
/// a pure shape transform, no business logic (spec §34). It relies on the loaded graph
/// (<c>Include</c>/<c>ThenInclude</c> in <c>RideRepository</c>) to avoid N+1 (spec §32).
///
/// <see cref="CreateRideRequest"/> → <see cref="Ride"/> copies only the client-supplied
/// trip details. The driver, seat count, status, timestamps, concurrency token and tag
/// links are all established by <c>RideService</c>.
/// </summary>
public class RideProfile : Profile
{
    public RideProfile()
    {
        CreateMap<User, RideDriverInfo>();
        CreateMap<Vehicle, RideVehicleInfo>();

        CreateMap<Ride, RideResponse>()
            .ForMember(d => d.Tags, o => o.MapFrom(s => s.RideTags.Select(rt => rt.Tag)));

        CreateMap<CreateRideRequest, Ride>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.DriverId, o => o.Ignore())
            .ForMember(d => d.AvailableSeats, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.StartedAt, o => o.Ignore())
            .ForMember(d => d.CompletedAt, o => o.Ignore())
            .ForMember(d => d.RowVersion, o => o.Ignore())
            .ForMember(d => d.Driver, o => o.Ignore())
            .ForMember(d => d.Vehicle, o => o.Ignore())
            .ForMember(d => d.Bookings, o => o.Ignore())
            .ForMember(d => d.RideTags, o => o.Ignore())
            .ForMember(d => d.Ratings, o => o.Ignore());
    }
}
