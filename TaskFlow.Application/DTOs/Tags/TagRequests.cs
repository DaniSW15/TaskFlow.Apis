namespace TaskFlow.Application.DTOs.Tags;

public sealed record CreateTagRequest(string Name, string Color = "#6366F1");
