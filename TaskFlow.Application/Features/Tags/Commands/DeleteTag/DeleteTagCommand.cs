using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tags.Commands.DeleteTag;

public sealed record DeleteTagCommand(Guid Id) : IRequest<Result>;
