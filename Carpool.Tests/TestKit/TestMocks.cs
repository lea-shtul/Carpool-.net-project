using Carpool.Core.Interfaces.Repositories;
using Moq;

namespace Carpool.Tests.TestKit;

/// <summary>Pre-wired mocks for the collaborators every service test needs.</summary>
public static class TestMocks
{
    /// <summary>
    /// An <see cref="IUnitOfWork"/> mock whose <c>SaveChangesAsync</c> returns 1 and whose
    /// <c>ExecuteInTransactionAsync</c> actually invokes the supplied delegate — so the
    /// transactional body of a service method really runs under test.
    /// </summary>
    public static Mock<IUnitOfWork> UnitOfWork()
    {
        var mock = new Mock<IUnitOfWork>();

        mock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        mock.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operation, CancellationToken ct) => operation(ct));

        return mock;
    }
}
