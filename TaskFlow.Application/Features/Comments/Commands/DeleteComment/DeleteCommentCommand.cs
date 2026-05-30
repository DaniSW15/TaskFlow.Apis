using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Comments.Commands.DeleteComment;

public sealed record DeleteCommentCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;
