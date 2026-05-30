using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Commands.AddTagToTask;

public sealed class AddTagToTaskCommandHandler : IRequestHandler<AddTagToTaskCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddTagToTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddTagToTaskCommand request, CancellationToken cancellationToken)
    {
        // Cargamos la tarea con su colección de Tags ya incluida
        var task = await _unitOfWork.Tasks.GetByIdWithTagsAsync(request.TaskItemId, cancellationToken);
        if (task is null)
            return Result.Failure(new Error("Tag.TaskNotFound", "The task does not exist."));

        var tag = await _unitOfWork.Tags.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null)
            return Result.Failure(new Error("Tag.NotFound", "The tag does not exist."));

        // Si ya tiene ese tag, no hacemos nada (idempotente)
        if (task.Tags.Any(t => t.Id == request.TagId))
            return Result.Success();

        // EF Core detecta el cambio en la colección y escribe en TaskItemTags
        task.Tags.Add(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
