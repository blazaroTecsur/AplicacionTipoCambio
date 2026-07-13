namespace Infor.Abstractions.Interfaces;

public interface IInforTokenService
{
    Task<string> ObtenerTokenAsync(CancellationToken ct = default);
}
