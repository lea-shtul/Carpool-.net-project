using AutoMapper;
using Carpool.Core.Dtos.Bookings;
using Carpool.Core.Entities;

namespace Carpool.Service.Mapping;

/// <summary>
/// AutoMapper profile for <see cref="Booking"/> (spec §34). On the request → entity map
/// only <c>NumberOfSeats</c> comes from the client; the ride, passenger, status and
/// timestamps are set by <c>BookingService</c> inside the booking transaction.
/// </summary>
public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<Booking, BookingResponse>();

        CreateMap<CreateBookingRequest, Booking>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.RideId, o => o.Ignore())
            .ForMember(d => d.PassengerId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CancelledAt, o => o.Ignore())
            .ForMember(d => d.Ride, o => o.Ignore())
            .ForMember(d => d.Passenger, o => o.Ignore());
    }
}
