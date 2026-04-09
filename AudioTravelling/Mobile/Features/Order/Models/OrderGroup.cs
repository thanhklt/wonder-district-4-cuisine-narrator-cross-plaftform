using AudioTravelling.Mobile.Features.Poi.Models;

namespace AudioTravelling.Mobile.Features.Order.Models;

public class OrderGroup : List<OrderItem>
{
    public string PoiName { get; private set; }
    public PoiModel Poi { get; private set; }

    public OrderGroup(string poiName, PoiModel poi, IEnumerable<OrderItem> collection) : base(collection)
    {
        PoiName = poiName;
        Poi = poi;
    }
}
