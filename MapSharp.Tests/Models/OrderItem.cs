namespace MapSharp.Tests.Models;

[MapFrom(typeof(OrderItemDto))]
[MapTo(typeof(OrderItemDto))]
public partial class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
