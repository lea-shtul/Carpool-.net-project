using AutoMapper;
using Carpool.Core.Dtos.Auth;
using Carpool.Core.Dtos.Users;
using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Carpool.Core.Exceptions;
using Carpool.Core.Interfaces.Repositories;
using Carpool.Core.Interfaces.Security;
using Carpool.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace Carpool.Service.Services;

/// <summary>
/// Implements <see cref="IAuthService"/> (spec §39, §40).
///
/// Registration stores only a hashed password (via <see cref="IPasswordHasher{TUser}"/>) and
/// always assigns the <see cref="UserRole.User"/> role — the role is never taken from client
/// input (spec §40). Login verifies the hash, rejects disabled accounts, and returns a
/// signed JWT. Failures for "unknown email" and "wrong password" are deliberately identical
/// so the API does not reveal which accounts exist.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException($"Email '{request.Email}' is already registered.");
        }

        var user = _mapper.Map<User>(request);
        user.Role = UserRole.User;              // spec §40 — never from the request
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        // Checked only after the password so a wrong password on a disabled account still
        // returns the generic failure and does not disclose that the account exists.
        if (!user.IsActive)
        {
            throw new UnauthorizedException("This account is disabled.");
        }

        var token = _jwtTokenGenerator.Generate(user);

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = _mapper.Map<UserResponse>(user),
        };
    }
}
