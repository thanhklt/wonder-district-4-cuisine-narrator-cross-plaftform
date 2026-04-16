using System.Collections.ObjectModel;
using System.Windows.Input;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;
using AudioTravelling.Mobile.ViewModels;

namespace AudioTravelling.Mobile.Features.Poi.ViewModels
{
    public class PoiListViewModel : BaseViewModel
    {
        private readonly IPoiApiService _poiApiService;

        public ObservableCollection<PoiResponse> Pois { get; } = new();

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoadPoisCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand OpenDetailCommand { get; }

        public PoiListViewModel(IPoiApiService poiApiService)
        {
            _poiApiService = poiApiService ?? throw new ArgumentNullException(nameof(poiApiService));

            LoadPoisCommand = new Command(async () => await ExecuteLoadPoisAsync());
            RefreshCommand = new Command(async () => await ExecuteRefreshAsync());
            OpenDetailCommand = new Command<PoiResponse>(async poi => await ExecuteOpenDetailAsync(poi));
        }

        public async Task LoadPoisAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var items = await _poiApiService.GetAllAsync();

                Pois.Clear();

                if (items == null)
                {
                    ErrorMessage = "Chưa có dữ liệu địa điểm.";
                    return;
                }

                foreach (var item in items)
                {
                    if (item != null)
                    {
                        Pois.Add(item);
                    }
                }

                if (Pois.Count == 0)
                {
                    ErrorMessage = "Chưa có dữ liệu địa điểm.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể tải danh sách địa điểm.";
                System.Diagnostics.Debug.WriteLine($"[PoiListViewModel.LoadPoisAsync] {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteLoadPoisAsync()
        {
            try
            {
                await LoadPoisAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể tải danh sách địa điểm.";
                System.Diagnostics.Debug.WriteLine($"[PoiListViewModel.ExecuteLoadPoisAsync] {ex}");
            }
        }

        private async Task ExecuteRefreshAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsRefreshing = true;
                await LoadPoisAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể làm mới danh sách địa điểm.";
                System.Diagnostics.Debug.WriteLine($"[PoiListViewModel.ExecuteRefreshAsync] {ex}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task ExecuteOpenDetailAsync(PoiResponse? poi)
        {
            if (poi == null)
                return;

            try
            {
                await Shell.Current.GoToAsync($"poidetail?poiId={poi.PoiId}");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể mở chi tiết địa điểm.";
                System.Diagnostics.Debug.WriteLine($"[PoiListViewModel.ExecuteOpenDetailAsync] {ex}");
            }
        }
    }
}