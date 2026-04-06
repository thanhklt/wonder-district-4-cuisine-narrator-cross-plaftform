using Mobile.Services;

namespace Mobile.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProfileData();
    }

    private void LoadProfileData()
    {
        EntEmail.Text = AuthService.CurrentEmail;
        EntFullName.Text = AuthService.CurrentName;
        EntPhone.Text = AuthService.CurrentPhone;

        LblAvatar.Text = AuthService.CurrentName.Length > 0
            ? AuthService.CurrentName[..1].ToUpper()
            : "U";
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        LblErrorMessage.IsVisible = false;

        string fullName = EntFullName.Text?.Trim() ?? string.Empty;
        string phone = EntPhone.Text?.Trim() ?? string.Empty;

        var (success, error) = AuthService.UpdateProfile(fullName, phone);

        if (!success)
        {
            LblErrorMessage.Text = error;
            LblErrorMessage.IsVisible = true;
            return;
        }

        // Cập nhật thành công
        await DisplayAlert("Thành công", "Thông tin tài khoản đã được cập nhật.", "OK");
        
        // Quay lại trang trước
        await Navigation.PopAsync();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
