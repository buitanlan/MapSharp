namespace MapSharp.Tests.Models;

[MapFrom(typeof(CustomerDto))]
[MapTo(typeof(CustomerDto))]
public partial class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Nested object - will auto-map using Address.MapFrom
    public Address? ShippingAddress { get; set; }
    public Address? BillingAddress { get; set; }
    
    // List of nested objects - will auto-map each item
    public List<OrderItem>? RecentOrders { get; set; }
    
    // Simple list - direct copy
    public List<string>? Tags { get; set; }
    
    // Property not in DTO - auto-ignored
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
