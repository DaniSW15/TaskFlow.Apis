using MediatR;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Shared.Common;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByBoardWithCursor;

/// <param name="BoardId">El tablero a paginar.</param>
/// <param name="RequestingUserId">Usuario que realiza la petición (validación de acceso).</param>
/// <param name="Cursor">
///   Cursor opaco (Base64) devuelto por la respuesta anterior.
///   Null o vacío inicia desde el principio.
/// </param>
/// <param name="PageSize">Ítems por página. Máximo 100.</param>
public sealed record GetTasksByBoardWithCursorQuery(
    Guid BoardId,
    Guid RequestingUserId,
    string? Cursor = null,
    int PageSize = 50) : IRequest<Result<CursorPaginatedList<TaskDto>>>;
