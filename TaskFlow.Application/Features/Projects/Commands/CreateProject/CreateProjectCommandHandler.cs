using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<Guid>(new Error("Project.ClientNotFound", "The specified client does not exist."));

        var analyst = await _unitOfWork.Users.GetByIdAsync(request.AnalystId, cancellationToken);
        if (analyst is null)
            return Result.Failure<Guid>(new Error("Project.AnalystNotFound", "The specified analyst does not exist."));

        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            ClientId = request.ClientId,
            AnalystId = request.AnalystId,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(project.Id);
    }
}
