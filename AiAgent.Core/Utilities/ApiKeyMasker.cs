namespace AiAgent.Core.Utilities;

public static class ApiKeyMasker
{
    public static string Mask(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "(empty)";
        }

        var trimmed = apiKey.Trim();
        if (trimmed.Length <= 4)
        {
            return "****";
        }

        var suffix = trimmed[^4..];
        return $"****{suffix}";
    }
}
