namespace Api.Modules.Owner.Interfaces;

using Api.Modules.Owner.DTOs;

public interface IOwnerPoiService
{
    Task<PoiResponse> CreatePoiAsync(CreatePoiRequest request, int ownerId);
    Task<List<OwnerPoiItemResponse>> GetMyPoisAsync(int ownerId);
}