namespace MapSharp.Tests.Models;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Nested object
    public AddressDto? ShippingAddress { get; set; }
    public AddressDto? BillingAddress { get; set; }
    
    // List of nested objects
    public List<OrderItemDto>? RecentOrders { get; set; }
    
    // Simple list
    public List<string>? Tags { get; set; }
}
