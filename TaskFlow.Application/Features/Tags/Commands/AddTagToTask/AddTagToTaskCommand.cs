using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Commands.AddTagToTask;

// Inserta una fila en la tabla TaskItemTags (relación N:M)
public sealed record AddTagToTaskCommand(Guid TaskItemId, Guid TagId) : IRequest<Result>;
