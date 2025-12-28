using MapSharp.Tests.Models;

namespace MapSharp.Tests;

public class NestedMappingTests
{
    [Fact]
    public void MapFrom_WithNestedObject_ShouldMapNestedProperties()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            ShippingAddress = new AddressDto
            {
                Street = "123 Main St",
                City = "New York",
                ZipCode = "10001",
                Country = "USA"
            }
        };

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer);
        Assert.Equal(dto.Id, customer.Id);
        Assert.Equal(dto.Name, customer.Name);
        Assert.NotNull(customer.ShippingAddress);
        Assert.Equal(dto.ShippingAddress.Street, customer.ShippingAddress.Street);
        Assert.Equal(dto.ShippingAddress.City, customer.ShippingAddress.City);
        Assert.Equal(dto.ShippingAddress.ZipCode, customer.ShippingAddress.ZipCode);
        Assert.Equal(dto.ShippingAddress.Country, customer.ShippingAddress.Country);
    }

    [Fact]
    public void MapFrom_WithNullNestedObject_ShouldHandleNull()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            ShippingAddress = null
        };

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer);
        Assert.Null(customer.ShippingAddress);
    }

    [Fact]
    public void MapFrom_WithMultipleNestedObjects_ShouldMapAll()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "Jane Smith",
            Email = "jane@example.com",
            ShippingAddress = new AddressDto
            {
                Street = "456 Oak Ave",
                City = "Los Angeles",
                ZipCode = "90001",
                Country = "USA"
            },
            BillingAddress = new AddressDto
            {
                Street = "789 Pine Rd",
                City = "Chicago",
                ZipCode = "60601",
                Country = "USA"
            }
        };

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer.ShippingAddress);
        Assert.NotNull(customer.BillingAddress);
        Assert.Equal("456 Oak Ave", customer.ShippingAddress.Street);
        Assert.Equal("789 Pine Rd", customer.BillingAddress.Street);
        Assert.NotEqual(customer.ShippingAddress.City, customer.BillingAddress.City);
    }

    [Fact]
    public void MapTo_WithNestedObject_ShouldMapNestedProperties()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            ShippingAddress = new Address
            {
                Street = "123 Main St",
                City = "New York",
                ZipCode = "10001",
                Country = "USA"
            }
        };

        // Act
        var dto = customer.MapTo();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(customer.Id, dto.Id);
        Assert.NotNull(dto.ShippingAddress);
        Assert.Equal(customer.ShippingAddress.Street, dto.ShippingAddress.Street);
        Assert.Equal(customer.ShippingAddress.City, dto.ShippingAddress.City);
    }

    [Fact]
    public void MapTo_WithNullNestedObject_ShouldHandleNull()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            ShippingAddress = null
        };

        // Act
        var dto = customer.MapTo();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.ShippingAddress);
    }

    [Fact]
    public void NestedMapping_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalDto = new CustomerDto
        {
            Id = 42,
            Name = "Test User",
            Email = "test@example.com",
            ShippingAddress = new AddressDto
            {
                Street = "Test Street",
                City = "Test City",
                ZipCode = "12345",
                Country = "Test Country"
            }
        };

        // Act
        var customer = Customer.MapFrom(originalDto);
        var resultDto = customer.MapTo();

        // Assert
        Assert.Equal(originalDto.Id, resultDto.Id);
        Assert.Equal(originalDto.Name, resultDto.Name);
        Assert.NotNull(resultDto.ShippingAddress);
        Assert.Equal(originalDto.ShippingAddress.Street, resultDto.ShippingAddress.Street);
        Assert.Equal(originalDto.ShippingAddress.City, resultDto.ShippingAddress.City);
    }
}
