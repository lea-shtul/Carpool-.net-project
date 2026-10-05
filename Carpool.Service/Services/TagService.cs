using AutoMapper;
using Carpool.Core.Dtos.Tags;
using Carpool.Core.Entities;
using Carpool.Core.Exceptions;
using Carpool.Core.Interfaces.Repositories;
using Carpool.Core.Interfaces.Services;

namespace Carpool.Service.Services;

/// <summary>
/// Implements <see cref="ITagService"/> (spec §30, §36). Beyond the base spec, this project
/// extends tags with a global/private split (see <see cref="Tag"/>): every caller sees the
/// global/seed tags plus their own private ones, and may create a new private tag for
/// themselves. Scoped name-uniqueness is enforced here, backed by the database's filtered
/// unique indexes (<c>TagConfiguration</c>) for the concurrent case.
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TagService(ITagRepository tagRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TagResponse>> GetVisibleToAsync(int currentUserId, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetVisibleToAsync(currentUserId, cancellationToken);
        return _mapper.Map<IEnumerable<TagResponse>>(tags);
    }

    public async Task<TagResponse> CreateAsync(int currentUserId, CreateTagRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await _tagRepository.NameExistsAsync(name, currentUserId, cancellationToken))
        {
            throw new ConflictException($"You already have a tag named '{name}'.");
        }

        var tag = new Tag { OwnerId = currentUserId, Name = name };

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TagResponse>(tag);
    }
}
