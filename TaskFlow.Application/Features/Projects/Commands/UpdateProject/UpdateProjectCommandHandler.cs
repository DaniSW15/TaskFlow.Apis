using MediatR;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);

        if (project is null)
            return Result.Failure(Error.NotFound);

        project.Title = request.Title;
        project.Description = request.Description;
        project.Status = request.Status;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
