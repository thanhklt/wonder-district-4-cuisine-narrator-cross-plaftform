using System.Collections.ObjectModel;
using AudioTravelling.Mobile.Features.Poi.Models;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;
using AudioTravelling.Mobile.ViewModels;

namespace AudioTravelling.Mobile.Features.Poi.ViewModels;

[QueryProperty(nameof(PoiId), "poiId")]
public class PoiDetailViewModel : BaseViewModel
{
    private readonly IPoiApiService _poiApiService;

    private string _poiId = string.Empty;
    public string PoiId
    {
        get => _poiId;
        set
        {
            if (SetProperty(ref _poiId, value))
            {
                _ = LoadPoiDetailAsync();
            }
        }
    }

    private PoiResponse? _poi;
    public PoiResponse? Poi
    {
        get => _poi;
        set => SetProperty(ref _poi, value);
    }

    private PoiImageModel? _coverImage;
    public PoiImageModel? CoverImage
    {
        get => _coverImage;
        set => SetProperty(ref _coverImage, value);
    }

    public ObservableCollection<PoiImageModel> Images { get; } = new();

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasImages => Images.Count > 0;

    public PoiDetailViewModel(IPoiApiService poiApiService)
    {
        _poiApiService = poiApiService;
    }

    public async Task LoadPoiDetailAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(PoiId))
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var poi = await _poiApiService.GetByIdAsync(PoiId);

            Poi = poi;

            Images.Clear();
            CoverImage = null;

            if (Poi == null)
            {
                ErrorMessage = "Không tải được chi tiết địa điểm.";
                OnPropertyChanged(nameof(HasImages));
                return;
            }

            LoadImagesFromPoi(Poi);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadImagesFromPoi(PoiResponse poi)
    {
        Images.Clear();
        CoverImage = null;

        if (poi.Images == null || poi.Images.Count == 0)
        {
            OnPropertyChanged(nameof(HasImages));
            return;
        }

        var imageModels = poi.Images
            .Select((url, index) => new PoiImageModel
            {
                ImageId = (index + 1).ToString(),
                PoiId = poi.PoiId,
                ImageUrl = url,
                LocalFilePath = null,
                DisplayOrder = index + 1,
                IsCover = index == 0
            })
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        foreach (var image in imageModels)
            Images.Add(image);

        CoverImage = imageModels.FirstOrDefault(x => x.IsCover)
                     ?? imageModels.FirstOrDefault();

        OnPropertyChanged(nameof(HasImages));
    }
}