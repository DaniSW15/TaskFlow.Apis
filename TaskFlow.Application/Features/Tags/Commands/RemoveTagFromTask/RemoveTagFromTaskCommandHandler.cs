using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Commands.RemoveTagFromTask;

public sealed class RemoveTagFromTaskCommandHandler : IRequestHandler<RemoveTagFromTaskCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveTagFromTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveTagFromTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithTagsAsync(request.TaskItemId, cancellationToken);
        if (task is null)
            return Result.Failure(new Error("Tag.TaskNotFound", "The task does not exist."));

        var tag = task.Tags.FirstOrDefault(t => t.Id == request.TagId);
        if (tag is null)
            return Result.Success(); // Idempotente: si no tiene el tag, no hay nada que hacer

        // EF Core detecta la eliminación de la colección y borra la fila de TaskItemTags
        task.Tags.Remove(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
