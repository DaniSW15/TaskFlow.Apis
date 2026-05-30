using MediatR;
using TaskFlow.Application.Common.Caching;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Commands.CreateTag;

public sealed class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CreateTagCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Tags.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(new Error("Tag.NameTaken", "A tag with this name already exists."));

        var tag = new Tag { Name = request.Name, Color = request.Color };

        await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidar el caché de tags para que el próximo GET refleje el nuevo tag
        _cache.Remove("tags:all");

        return Result.Success(tag.Id);
    }
}
