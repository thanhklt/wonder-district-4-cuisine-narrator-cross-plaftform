using Mobile.ViewModels;
using ZXing.Net.Maui;

namespace Mobile.Views;

public partial class QrScanPage : ContentPage
{
    private readonly QrScanViewModel _vm;
    private bool _processed;

    public QrScanPage(QrScanViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;

        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _processed = false;
        _vm.RetryScanCommand.Execute(null);
    }

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_processed) return;
        var code = e.Results.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(code)) return;

        _processed = true;
        await MainThread.InvokeOnMainThreadAsync(() =>
            _vm.OnQrDetectedAsync(code));
    }
}
