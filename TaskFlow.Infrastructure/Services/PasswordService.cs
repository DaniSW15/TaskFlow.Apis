using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public sealed class PasswordService : IPasswordService
{
    // Work factor of 12 provides a good balance between security and performance
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
