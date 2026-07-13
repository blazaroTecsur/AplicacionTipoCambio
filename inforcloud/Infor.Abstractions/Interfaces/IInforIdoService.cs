using Infor.Abstractions.DTOs;

namespace Infor.Abstractions.Interfaces;

public interface IInforIdoService
{
    Task<IdoResponse> LoadAsync(
        string ido,
        string? properties = null,
        string? filter     = null,
        int    recordCap   = 0,
        string? orderBy    = null,
        CancellationToken ct = default);

    Task<IdoResponse> InsertItemAsync(
        string ido,
        IEnumerable<IdoProperty> properties,
        bool refreshAfterSave = false,
        CancellationToken ct  = default);

    Task<IdoResponse> UpdateItemAsync(
        string ido,
        string itemId,
        IEnumerable<IdoProperty> properties,
        bool refreshAfterSave = false,
        CancellationToken ct  = default);
}
