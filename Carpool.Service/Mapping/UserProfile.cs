using AutoMapper;
using Carpool.Core.Dtos.Auth;
using Carpool.Core.Dtos.Users;
using Carpool.Core.Entities;

namespace Carpool.Service.Mapping;

/// <summary>
/// AutoMapper profile for <see cref="User"/> (spec §34).
///
/// <see cref="UserResponse"/> deliberately has no <c>PasswordHash</c> member, so that
/// field is never projected out. On <see cref="RegisterRequest"/> → <see cref="User"/>
/// only the plain profile fields are copied; the password hash, role, active flag,
/// created timestamp and key are all set by the Service layer, not here (spec §34 — no
/// business logic in profiles).
/// </summary>
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>();

        CreateMap<RegisterRequest, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Vehicles, o => o.Ignore())
            .ForMember(d => d.RidesAsDriver, o => o.Ignore())
            .ForMember(d => d.Bookings, o => o.Ignore())
            .ForMember(d => d.RatingsGiven, o => o.Ignore())
            .ForMember(d => d.RatingsReceived, o => o.Ignore());
    }
}
