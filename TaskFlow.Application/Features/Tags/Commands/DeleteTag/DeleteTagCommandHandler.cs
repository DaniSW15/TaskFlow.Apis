using MediatR;
using TaskFlow.Application.Common.Caching;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Commands.DeleteTag;

public sealed class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteTagCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _unitOfWork.Tags.GetByIdAsync(request.Id, cancellationToken);

        if (tag is null)
            return Result.Failure(Error.NotFound);

        _unitOfWork.Tags.Delete(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidar el caché de tags
        _cache.Remove("tags:all");

        return Result.Success();
    }
}
