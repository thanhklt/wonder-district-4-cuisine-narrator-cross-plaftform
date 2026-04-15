using Api.Modules.Admin.DTOs;

namespace Api.Modules.Admin.Interfaces;

public interface IAdminPoiService
{
    Task<List<PendingPoiResponse>> GetPendingPoisAsync(CancellationToken cancellationToken = default);
    Task ApprovePoiAsync(int poiId, int adminUserId, CancellationToken cancellationToken = default);
    Task RejectPoiAsync(int poiId, int adminUserId, string note, CancellationToken cancellationToken = default);
}