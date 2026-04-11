namespace AudioTravelling.Mobile.Services.Api;
public class ApiOptions
{
#if ANDROID
    public const string BaseUrl = "http://10.0.2.2:5184/";
#else
    public const string BaseUrl = "http://localhost:5184/";
#endif
}
