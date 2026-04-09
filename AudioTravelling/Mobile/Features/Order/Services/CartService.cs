using System.Collections.ObjectModel;
using AudioTravelling.Mobile.Features.Order.Models;
using AudioTravelling.Mobile.Features.Poi.Models;

namespace AudioTravelling.Mobile.Features.Order.Services;

// ── CART SERVICE (Singleton) ──────────────────────────────────────────────────
public class CartService
{
    public static readonly CartService Instance = new();
    private CartService() { }

    private readonly List<OrderItem> _items = new();

    public IReadOnlyList<OrderItem> Items => _items;

    // Added for UI grouping
    public ObservableCollection<OrderGroup> GroupedItems { get; } = new();

    public int    TotalCount => _items.Sum(i => i.Quantity);
    public decimal TotalPrice => _items.Sum(i => i.LineTotal);
    public string TotalPriceLabel => $"{TotalPrice:#,##0} VNĐ";

    // Events so UI can react
    public event Action? CartChanged;

    public void Add(PoiModel poi, FoodItem food)
    {
        var existing = _items.FirstOrDefault(i => i.Food.Id == food.Id);
        if (existing is not null)
            existing.Quantity++;
        else
            _items.Add(new OrderItem { Poi = poi, Food = food, Quantity = 1 });
        
        UpdateGroupedItems();
        CartChanged?.Invoke();
    }

    public void Remove(OrderItem item)
    {
        if (item.Quantity > 1)
            item.Quantity--;
        else
            _items.Remove(item);
        
        UpdateGroupedItems();
        CartChanged?.Invoke();
    }

    public void Clear()
    {
        _items.Clear();
        UpdateGroupedItems();
        CartChanged?.Invoke();
    }

    private void UpdateGroupedItems()
    {
        GroupedItems.Clear();
        var groups = _items.GroupBy(i => i.Poi).ToList();
        foreach (var g in groups)
        {
            GroupedItems.Add(new OrderGroup(g.Key.Name, g.Key, g));
        }
    }
}
