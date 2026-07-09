namespace TasaCambio.Infrastructure.Infor;

internal sealed class InforSettings
{
    public string SsoBaseUrl    { get; init; } = string.Empty;
    public string TokenEndpoint { get; init; } = string.Empty;
    public string ClientId      { get; init; } = string.Empty;
    public string ClientSecret  { get; init; } = string.Empty;
    public string IdoBaseUrl    { get; init; } = string.Empty;
    public string AppId         { get; init; } = string.Empty;
    public string MonedaBase    { get; init; } = "PEN";
}
