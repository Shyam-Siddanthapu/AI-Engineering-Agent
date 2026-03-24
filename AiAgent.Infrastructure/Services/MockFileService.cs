using System.Collections.Concurrent;

namespace AiAgent.Infrastructure.Services;

public sealed class MockFileService
{
    private readonly ConcurrentDictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);

    public MockFileService()
    {
        _files["OrderService.cs"] = """
            namespace Sample.Services;

            public sealed class OrderService
            {
                public decimal CalculateTotal(decimal subtotal, decimal tax)
                    => subtotal + tax;
            }
            """;

        _files["UserService.cs"] = """
            namespace Sample.Services;

            public sealed class UserService
            {
                public string GetDisplayName(string firstName, string lastName)
                    => $"{firstName} {lastName}";
            }
            """;
    }

    public string? GetFile(string path)
    {
        return _files.TryGetValue(path, out var content) ? content : null;
    }

    public void UpdateFile(string path, string content)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("File path is required.", nameof(path));
        }

        _files[path] = content ?? string.Empty;
    }

    public IReadOnlyDictionary<string, string> GetAllFiles() => _files;
}
