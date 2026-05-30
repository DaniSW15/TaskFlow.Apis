using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Commands.CreateTag;

public sealed record CreateTagCommand(string Name, string Color = "#6366F1") : IRequest<Result<Guid>>;
