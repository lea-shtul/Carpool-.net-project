using AutoMapper;
using Carpool.Service.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace Carpool.Tests.TestKit;

/// <summary>
/// Builds a real <see cref="IMapper"/> from the production profiles, so service tests
/// exercise the actual entity → DTO mapping instead of a hand-stubbed <c>Mock&lt;IMapper&gt;</c>.
/// </summary>
public static class TestMapper
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(TagProfile).Assembly),
            NullLoggerFactory.Instance);

        return configuration.CreateMapper();
    }
}
