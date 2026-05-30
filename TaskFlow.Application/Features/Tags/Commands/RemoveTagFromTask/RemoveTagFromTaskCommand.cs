using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Commands.RemoveTagFromTask;

// Elimina una fila de la tabla TaskItemTags (relación N:M)
public sealed record RemoveTagFromTaskCommand(Guid TaskItemId, Guid TagId) : IRequest<Result>;
