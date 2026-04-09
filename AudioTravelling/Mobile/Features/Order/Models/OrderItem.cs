using AudioTravelling.Mobile.Features.Poi.Models;

namespace AudioTravelling.Mobile.Features.Order.Models;

public class OrderItem
{
    public PoiModel    Poi      { get; set; } = null!;
    public FoodItem    Food     { get; set; } = null!;
    public int         Quantity { get; set; } = 1;

    public decimal LineTotal   => Food.Price * Quantity;
    public string  LineTotalLabel => $"{LineTotal:#,##0} VNĐ";
    public string  QuantityLabel  => Quantity.ToString();
}
