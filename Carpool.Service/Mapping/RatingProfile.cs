using AutoMapper;
using Carpool.Core.Dtos.Ratings;
using Carpool.Core.Entities;

namespace Carpool.Service.Mapping;

/// <summary>
/// AutoMapper profile for <see cref="Rating"/> (spec §34). On the request → entity map
/// only <c>Score</c> and <c>Comment</c> come from the client; the ride and reviewer come
/// from the route/JWT and the driver is copied from the ride by <c>RatingService</c>.
/// </summary>
public class RatingProfile : Profile
{
    public RatingProfile()
    {
        CreateMap<Rating, RatingResponse>();

        CreateMap<CreateRatingRequest, Rating>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.RideId, o => o.Ignore())
            .ForMember(d => d.ReviewerId, o => o.Ignore())
            .ForMember(d => d.DriverId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Ride, o => o.Ignore())
            .ForMember(d => d.Reviewer, o => o.Ignore())
            .ForMember(d => d.Driver, o => o.Ignore());
    }
}
