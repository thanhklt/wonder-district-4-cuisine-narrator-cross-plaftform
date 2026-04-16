using AudioTravelling.Mobile.Data.Models;
using AudioTravelling.Mobile.Features.Poi.Models;

namespace AudioTravelling.Mobile.Features.Poi.Mappers;

public static class PoiModelMapper
{
    public static PoiImageModel ToPoiImageModel(CachedPoiImage entity)
    {
        return new PoiImageModel
        {
            ImageId = entity.ImageId,
            PoiId = entity.PoiId,
            ImageUrl = entity.ImageUrl,
            LocalFilePath = entity.LocalFilePath,
            DisplayOrder = entity.DisplayOrder,
            IsCover = entity.IsCover
        };
    }
}