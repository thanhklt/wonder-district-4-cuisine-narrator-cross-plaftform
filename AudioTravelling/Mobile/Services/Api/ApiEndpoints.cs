namespace AudioTravelling.Mobile.Services.Api;

public static class ApiEndpoints
{
    // Auth endpoints
    public const string Auth_Login = "/api/auth/login";
    public const string Auth_Register = "/api/auth/signup";
    public const string Auth_RefreshToken = "/api/auth/refresh-token";
    public const string Auth_Logout = "/api/auth/logout";

    // User endpoints
    public const string User_Me = "/api/users/me";
    public const string User_GetById = "/api/users/{0}";
    public const string User_Update = "/api/users/{0}";
    public const string User_GetAll = "/api/users";

    // POI endpoints
    public const string Poi_GetAll = "/api/pois";
    public const string Poi_GetById = "/api/pois/{0}";
    public const string Poi_Search = "/api/pois/search";
    public const string Poi_GetNearby = "/api/pois/nearby";
}
