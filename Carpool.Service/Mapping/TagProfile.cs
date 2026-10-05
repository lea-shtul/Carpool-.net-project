using AutoMapper;
using Carpool.Core.Dtos.Tags;
using Carpool.Core.Entities;

namespace Carpool.Service.Mapping;

/// <summary>
/// AutoMapper profile for <see cref="Tag"/> (spec §34). The owner is never taken from the
/// client — <c>OwnerId</c> is set from the JWT in <c>TagService.CreateAsync</c> (spec §43).
/// </summary>
public class TagProfile : Profile
{
    public TagProfile()
    {
        CreateMap<Tag, TagResponse>()
            .ForMember(d => d.IsPrivate, o => o.MapFrom(s => s.OwnerId != null));
    }
}
