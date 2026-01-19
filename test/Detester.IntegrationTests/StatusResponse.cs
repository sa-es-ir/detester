namespace Detester.IntegrationTests;

public sealed class StatusResponse
{
    public string? Status { get; set; }

    public string[]? Tags { get; set; }

    public int Id { get; set; }

    public string? Message { get; set; }
}
