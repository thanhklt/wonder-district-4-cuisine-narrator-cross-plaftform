using Mobile.Models;

namespace Mobile.Services;

/// <summary>
/// Static dummy POI data for Dotonbori, sorted by distance.
/// Replace with real API/GPS calls in production.
/// </summary>
public static class PoiService
{
    // ── Global audio flag ──────────────────────────────────────────────────
    private static bool _audioEnabled = true;
    public static bool AudioEnabled
    {
        get => _audioEnabled;
        set
        {
            if (_audioEnabled == value) return;
            _audioEnabled = value;
            AudioEnabledChanged?.Invoke(value);
        }
    }
    public static event Action<bool>? AudioEnabledChanged;

    // ── Dummy food menus ───────────────────────────────────────────────────
    private static List<FoodItem> TakoyakiMenu => new()
    {
        new() { Id=1,  Name="Takoyaki 6 viên",   Description="Bạch tuộc, sốt Worcestershire, katsuobushi",  Price=500,  ImageSource="takoyaki_img.png" },
        new() { Id=2,  Name="Takoyaki 12 viên",  Description="Hộp lớn đầy đặn cho 2 người",                 Price=900,  ImageSource="takoyaki_img.png" },
        new() { Id=3,  Name="Set đặc biệt Kukuru",Description="Takoyaki + nước dashi ấm + khăn lạnh",       Price=1200, ImageSource="takoyaki_img.png" },
    };

    private static List<FoodItem> RamenMenu => new()
    {
        new() { Id=10, Name="Ramen Tonkotsu",   Description="Nước hầm xương 12 tiếng, thịt charsiu mềm",   Price=850,  ImageSource="ramen_img.png" },
        new() { Id=11, Name="Ramen Shoyu",       Description="Nước dashi thanh nhẹ, trứng lòng đào",        Price=780,  ImageSource="ramen_img.png" },
        new() { Id=12, Name="Tsukemen",          Description="Mì chấm sốt đậm đà kiểu Tokyo",               Price=920,  ImageSource="ramen_img.png" },
    };

    private static List<FoodItem> KushikatsuMenu => new()
    {
        new() { Id=20, Name="Set Kushikatsu 5 que",   Description="Hải sản & rau củ chiên giòn",            Price=650,  ImageSource="kushikatsu_img.png" },
        new() { Id=21, Name="Set Kushikatsu 10 que",  Description="Đặc sản Osaka, chấm sốt ngọt",           Price=1100, ImageSource="kushikatsu_img.png" },
        new() { Id=22, Name="Kushikatsu tôm hùm",     Description="Tôm hùm Canada, bơ chanh",               Price=1800, ImageSource="kushikatsu_img.png" },
    };

    private static List<FoodItem> OkonomiyakiMenu => new()
    {
        new() { Id=30, Name="Okonomiyaki tôm mực",    Description="Bánh xèo Nhật kiểu Osaka, hải sản tươi", Price=950,  ImageSource="okonomiyaki_img.png" },
        new() { Id=31, Name="Okonomiyaki lợn phô mai",Description="Phô mai tan chảy, thịt lợn ba chỉ",      Price=980,  ImageSource="okonomiyaki_img.png" },
        new() { Id=32, Name="Modanyaki",              Description="Okonomiyaki + mì soba giòn",              Price=1050, ImageSource="okonomiyaki_img.png" },
    };

    private static List<FoodItem> IceCreamMenu => new()
    {
        new() { Id=40, Name="Kem matcha xoắn",        Description="Kem tươi matcha Uji trên ốc quế giòn",   Price=450,  ImageSource="icecream_img.png" },
        new() { Id=41, Name="Kem miso",               Description="Vị ngọt muối đặc trưng Nhật Bản",         Price=480,  ImageSource="icecream_img.png" },
        new() { Id=42, Name="Parfait Dotonbori",      Description="Tổng hợp 5 loại kem + đào mật",           Price=750,  ImageSource="icecream_img.png" },
    };

    private static List<FoodItem> SushiMenu => new()
    {
        new() { Id=50, Name="Omakase 8 miếng",        Description="Sushi cá ngừ béo, tôm, nhum biển",        Price=2800, ImageSource="sushi_img.png" },
        new() { Id=51, Name="Gunkan Ikura",            Description="Trứng cá hồi tươi",                       Price=680,  ImageSource="sushi_img.png" },
        new() { Id=52, Name="Temaki tôm",              Description="Cuộn tay hình nón, ăn liền",              Price=520,  ImageSource="sushi_img.png" },
    };

    private static List<FoodItem> GydozaMenu => new()
    {
        new() { Id=60, Name="Gyoza chiên 6 cái",      Description="Vỏ giòn rộp, nhân thịt heo và hẹ",       Price=380,  ImageSource="gyoza_img.png" },
        new() { Id=61, Name="Gyoza hấp 8 cái",        Description="Mềm mại, chấm giấm ponzu",               Price=420,  ImageSource="gyoza_img.png" },
        new() { Id=62, Name="Gyoza Wagyu đặc biệt",   Description="Thịt bò Wagyu A5 chính hãng",            Price=980,  ImageSource="gyoza_img.png" },
    };

    // ── POI catalogue ──────────────────────────────────────────────────────
    private static readonly List<PoiModel> _all = new()
    {
        new PoiModel
        {
            Id=1, Name="Takoyaki Kukuru", Category="Đồ ăn vặt",
            DistanceMeters=48,  Rating=4.9, IsOpen=true,
            ImageSource="takoyaki_img.png",
            Description="Một trong những quán Takoyaki lâu đời nhất Dotonbori. Những viên bạch tuộc nướng tròn trịa, nóng hổi, phủ đầy sốt đặc trưng và rắc katsuobushi thơm lừng – trải nghiệm không thể bỏ qua khi đến Osaka!",
            AudioScript="Chào mừng đến với Takoyaki Kukuru, điểm dừng đầu tiên trong hành trình ẩm thực Dotonbori của bạn...",
            Menu=TakoyakiMenu
        },
        new PoiModel
        {
            Id=2, Name="Ichiran Ramen Dotonbori", Category="Ramen",
            DistanceMeters=120, Rating=4.7, IsOpen=true,
            ImageSource="ramen_img.png",
            Description="Chuỗi ramen nổi tiếng thế giới với phong cách ăn một mình độc đáo. Mỗi thực khách ngồi trong ô riêng, tập trung hoàn toàn vào tô ramen tonkotsu đậm đà.",
            AudioScript="Bạn đang đứng trước Ichiran Ramen, nơi ẩm thực trở thành một hành trình cá nhân đặc biệt...",
            Menu=RamenMenu
        },
        new PoiModel
        {
            Id=3, Name="Kushikatsu Daruma", Category="Kushikatsu",
            DistanceMeters=200, Rating=4.8, IsOpen=true,
            ImageSource="kushikatsu_img.png",
            Description="Thương hiệu Kushikatsu biểu tượng của Osaka từ 1929. Quy tắc vàng: không được chấm đôi! Hải sản và rau củ tươi, bao bột panko vàng ruộm, chiên ngập dầu.",
            AudioScript="Kushikatsu Daruma, thành lập năm 1929, là linh hồn ẩm thực của Osaka...",
            Menu=KushikatsuMenu
        },
        new PoiModel
        {
            Id=4, Name="Kiji Okonomiyaki", Category="Okonomiyaki",
            DistanceMeters=340, Rating=4.6, IsOpen=false,
            ImageSource="okonomiyaki_img.png",
            Description="Nghệ nhân làm Okonomiyaki theo phong cách Osaka cổ truyền từ những năm 1970. Bánh được nướng trực tiếp trên bàn sắt nóng trước mắt thực khách.",
            AudioScript="Kiji Okonomiyaki, nơi lưu giữ truyền thống ẩm thực Osaka hơn 50 năm...",
            Menu=OkonomiyakiMenu
        },
        new PoiModel
        {
            Id=5, Name="Cremia Ice Cream", Category="Dessert",
            DistanceMeters=480, Rating=4.5, IsOpen=true,
            ImageSource="icecream_img.png",
            Description="Kem tươi xоắn mềm mịn được làm từ sữa Hokkaido cao cấp. Ốc quế giòn tan tuyệt vời, trở thành biểu tượng check-in của khu phố Dotonbori.",
            AudioScript="Cremia Ice Cream, nơi từng cây kem là một tác phẩm nghệ thuật vị giác...",
            Menu=IceCreamMenu
        },
        new PoiModel
        {
            Id=6, Name="Sushi Saito Premium", Category="Sushi",
            DistanceMeters=620, Rating=4.95, IsOpen=true,
            ImageSource="sushi_img.png",
            Description="Nhà hàng sushi omakase cao cấp với đầu bếp Saito Kenji, nguyên là đầu bếp trưởng tại Ginza Tokyo. Nguyên liệu tươi nhập mỗi sáng từ chợ Kuromon.",
            AudioScript="Sushi Saito Premium - nơi nghệ thuật làm sushi đạt đến đỉnh cao...",
            Menu= SushiMenu
        },
        new PoiModel
        {
            Id=7, Name="Gyoza no Ohsho", Category="Gyoza",
            DistanceMeters=850, Rating=4.4, IsOpen=true,
            ImageSource="gyoza_img.png",
            Description="Chuỗi gyoza bình dân nhất Nhật Bản với công thức không đổi từ 1967. Vỏ bánh giòn phồng vàng ươm, nhân thịt heo và hẹ thơm lừng, giá cực kỳ phải chăng.",
            AudioScript="Gyoza no Ohsho, thương hiệu bánh há cảo quốc dân của Nhật Bản...",
            Menu=GydozaMenu
        },
    };

    /// <summary>Returns all POIs sorted by distance ascending.</summary>
    public static IReadOnlyList<PoiModel> GetSortedByDistance() =>
        _all.OrderBy(p => p.DistanceMeters).ToList();

    /// <summary>Find a single POI by ID.</summary>
    public static PoiModel? GetById(int id) => _all.FirstOrDefault(p => p.Id == id);
}
