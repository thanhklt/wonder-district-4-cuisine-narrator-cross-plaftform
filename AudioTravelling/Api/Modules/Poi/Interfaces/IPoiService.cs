using Api.Modules.Poi.DTOs;

namespace Api.Modules.Poi.Interfaces;

public interface IPoiService
{
    Task<List<PoiListItemResponse>> GetActivePoisAsync();
    Task<PoiDetailResponse?> GetPoiByIdAsync(int poiId);
}