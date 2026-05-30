using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);

        if (project is null)
            return Result.Failure(Error.NotFound);

        _unitOfWork.Projects.Delete(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
