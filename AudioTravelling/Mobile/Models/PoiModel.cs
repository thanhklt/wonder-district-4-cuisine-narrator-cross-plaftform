namespace Mobile.Models;

// ── POI ──────────────────────────────────────────────────────────────────────
public class PoiModel
{
    public int    Id             { get; set; }
    public string Name          { get; set; } = string.Empty;
    public string Category      { get; set; } = string.Empty;
    public string Description   { get; set; } = string.Empty;
    public string ImageSource   { get; set; } = string.Empty;
    public double DistanceMeters{ get; set; }
    public double Rating        { get; set; }
    public bool   IsOpen        { get; set; }
    public string AudioScript   { get; set; } = string.Empty;

    // Computed display helpers
    public string DistanceLabel => DistanceMeters < 1000
        ? $"{(int)DistanceMeters} m"
        : $"{DistanceMeters / 1000:F1} km";

    public string RatingLabel   => $"★ {Rating:F1}";
    public string StatusLabel   => IsOpen ? "Đang mở cửa" : "Đã đóng cửa";
    public Color  StatusColor   => IsOpen
        ? Color.FromArgb("#10B981")
        : Color.FromArgb("#EF4444");

    public List<FoodItem> Menu  { get; set; } = new();
}

// ── FOOD ITEM ─────────────────────────────────────────────────────────────────
public class FoodItem
{
    public int    Id          { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price      { get; set; }
    public string ImageSource { get; set; } = string.Empty;

    public string PriceLabel  => $"{Price:#,##0} ¥";
}

// ── ORDER ITEM ────────────────────────────────────────────────────────────────
public class OrderItem
{
    public PoiModel    Poi      { get; set; } = null!;
    public FoodItem    Food     { get; set; } = null!;
    public int         Quantity { get; set; } = 1;

    public decimal LineTotal   => Food.Price * Quantity;
    public string  LineTotalLabel => $"{LineTotal:#,##0} ¥";
    public string  QuantityLabel  => Quantity.ToString();
}

// ── CART SERVICE (Singleton) ──────────────────────────────────────────────────
public class CartService
{
    public static readonly CartService Instance = new();
    private CartService() { }

    private readonly List<OrderItem> _items = new();

    public IReadOnlyList<OrderItem> Items => _items;

    public int    TotalCount => _items.Sum(i => i.Quantity);
    public decimal TotalPrice => _items.Sum(i => i.LineTotal);
    public string TotalPriceLabel => $"{TotalPrice:#,##0} ¥";

    // Events so UI can react
    public event Action? CartChanged;

    public void Add(PoiModel poi, FoodItem food)
    {
        var existing = _items.FirstOrDefault(i => i.Food.Id == food.Id);
        if (existing is not null)
            existing.Quantity++;
        else
            _items.Add(new OrderItem { Poi = poi, Food = food, Quantity = 1 });
        CartChanged?.Invoke();
    }

    public void Remove(OrderItem item)
    {
        if (item.Quantity > 1)
            item.Quantity--;
        else
            _items.Remove(item);
        CartChanged?.Invoke();
    }

    public void Clear()
    {
        _items.Clear();
        CartChanged?.Invoke();
    }
}
