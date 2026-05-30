using MediatR;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Comments.Commands.AddComment;

public sealed record AddCommentCommand(
    Guid TaskItemId,
    string Content,
    Guid AuthorId) : IRequest<Result<Guid>>;
