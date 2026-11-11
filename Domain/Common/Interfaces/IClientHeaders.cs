namespace Domain.Common.Interfaces
{
    public interface IClientHeaders
    {
        string ClientIdempotencyKey { get; }

        string GetHeader(string headerName);
    }
}
