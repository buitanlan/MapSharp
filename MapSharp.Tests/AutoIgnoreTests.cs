using MapSharp.Tests.Models;

namespace MapSharp.Tests;

public class AutoIgnoreTests
{
    [Fact]
    public void MapFrom_WithPropertyNotInSource_ShouldAutoIgnore()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com"
        };

        var beforeMapping = DateTime.UtcNow;

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer);
        Assert.Equal(dto.Id, customer.Id);
        Assert.Equal(dto.Name, customer.Name);
        // CreatedAt is not in DTO, should be auto-ignored and use default value
        Assert.True(customer.CreatedAt >= beforeMapping);
    }

    [Fact]
    public void MapTo_WithPropertyNotInTarget_ShouldAutoIgnore()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedAt = new DateTime(2024, 1, 1)
        };

        // Act
        var dto = customer.MapTo();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(customer.Id, dto.Id);
        Assert.Equal(customer.Name, dto.Name);
        // CustomerDto doesn't have CreatedAt - it should be auto-ignored
    }

    [Fact]
    public void MapFrom_WithMismatchedPropertyTypes_ShouldNotCauseError()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "Test",
            Email = "test@example.com",
            ShippingAddress = new AddressDto
            {
                Street = "123 Main St",
                City = "Test City",
                ZipCode = "12345",
                Country = "Test Country"
            }
        };

        // Act - should not throw even though nested types are different
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer);
        Assert.NotNull(customer.ShippingAddress);
    }

    [Fact]
    public void MapFrom_WithExplicitIgnoreAttribute_ShouldIgnoreProperty()
    {
        // Arrange
        var userDto = new UserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var userProfile = UserProfile.MapFrom(userDto);

        // Assert
        Assert.NotNull(userProfile);
        Assert.Equal(userDto.Username, userProfile.Name);
        Assert.Equal(userDto.Email, userProfile.Email);
        // CreatedAt has [MapProperty(Ignore = true)] - should not be mapped from source
    }

    [Fact]
    public void MapFrom_MultiplePropertiesNotInSource_AllAutoIgnored()
    {
        // This test ensures that having multiple properties not in source doesn't cause issues
        
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "Test",
            Email = "test@example.com"
        };

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer);
        // All these should work even though many properties aren't mapped
        Assert.Equal(1, customer.Id);
        Assert.Null(customer.ShippingAddress);
        Assert.Null(customer.BillingAddress);
        Assert.Null(customer.RecentOrders);
        Assert.Null(customer.Tags);
    }
}
