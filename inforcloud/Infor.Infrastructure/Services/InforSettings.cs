namespace Infor.Infrastructure.Services;

public sealed class InforSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string SsoBaseUrl           { get; init; } = string.Empty;
    public string Tenant               { get; init; } = string.Empty;
    public string TokenEndpoint =>
        $"{SsoBaseUrl.TrimEnd('/')}/{Tenant}/as/token.oauth2";
    public string ClientId             { get; init; } = string.Empty;
    public string ClientSecret         { get; init; } = string.Empty;
    public string ServiceAccountKey    { get; init; } = string.Empty;
    public string ServiceAccountSecret { get; init; } = string.Empty;
    public string IdoBaseUrl =>
        $"{BaseUrl.TrimEnd('/')}/{Tenant}/{AppId}/IDORequestService/ido/";
    public string AppId                { get; init; } = string.Empty;
    public string MongooseConfig       { get; init; } = string.Empty;
    public string MonedaBase           { get; init; } = "PEN";
}
