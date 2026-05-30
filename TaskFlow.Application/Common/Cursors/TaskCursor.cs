using System.Globalization;
using System.Text;

namespace TaskFlow.Application.Common.Cursors;

/// <summary>
/// Cursor compuesto por (CreatedAt, Id) para paginar TaskItems.
///
/// El cursor se serializa como "ISO8601|Guid" y se codifica en Base64
/// para que sea opaco para el cliente — este no debe interpretar su contenido.
///
/// Usar dos campos evita ambigüedad cuando múltiples filas tienen
/// exactamente el mismo CreatedAt (lo que ocurre con inserciones en batch).
/// </summary>
public sealed record TaskCursor(DateTime CreatedAt, Guid Id)
{
    /// <summary>Serializa el cursor a un string Base64 opaco.</summary>
    public string Encode()
    {
        var raw = $"{CreatedAt:O}|{Id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    /// <summary>
    /// Decodifica un cursor recibido del cliente.
    /// Retorna null si el cursor es inválido (corrupto, alterado o vacío).
    /// </summary>
    public static TaskCursor? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|', 2);

            if (parts.Length != 2)
                return null;

            var createdAt = DateTime.Parse(parts[0], null, DateTimeStyles.RoundtripKind);
            var id = Guid.Parse(parts[1]);

            return new TaskCursor(createdAt, id);
        }
        catch
        {
            // Un cursor inválido simplemente inicia desde el principio
            return null;
        }
    }
}
