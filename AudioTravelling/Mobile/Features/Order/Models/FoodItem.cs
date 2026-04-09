namespace AudioTravelling.Mobile.Features.Order.Models;

public class FoodItem
{
    public int    Id          { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price      { get; set; }
    public string ImageSource { get; set; } = string.Empty;

    public string PriceLabel  => $"{Price:#,##0} VNĐ";
}
