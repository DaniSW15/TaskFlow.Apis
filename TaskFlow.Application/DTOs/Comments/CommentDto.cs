namespace TaskFlow.Application.DTOs.Comments;

public sealed record CommentDto(
    Guid Id,
    string Content,
    Guid TaskItemId,
    Guid AuthorId,
    string AuthorFullName,
    DateTime CreatedAt);
