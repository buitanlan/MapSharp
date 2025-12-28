using MapSharp.Tests.Models;

namespace MapSharp.Tests;

public class ListMappingTests
{
    [Fact]
    public void MapFrom_WithListOfNestedObjects_ShouldMapAllItems()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            RecentOrders = new List<OrderItemDto>
            {
                new() { ProductId = 1, ProductName = "Laptop", Quantity = 1, UnitPrice = 999.99m },
                new() { ProductId = 2, ProductName = "Mouse", Quantity = 2, UnitPrice = 29.99m },
                new() { ProductId = 3, ProductName = "Keyboard", Quantity = 1, UnitPrice = 79.99m }
            }
        };

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer);
        Assert.NotNull(customer.RecentOrders);
        Assert.Equal(3, customer.RecentOrders.Count);
        Assert.Equal("Laptop", customer.RecentOrders[0].ProductName);
        Assert.Equal(999.99m, customer.RecentOrders[0].UnitPrice);
        Assert.Equal("Mouse", customer.RecentOrders[1].ProductName);
        Assert.Equal(2, customer.RecentOrders[1].Quantity);
    }

    [Fact]
    public void MapFrom_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            RecentOrders = new List<OrderItemDto>()
        };

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer.RecentOrders);
        Assert.Empty(customer.RecentOrders);
    }

    [Fact]
    public void MapFrom_WithNullList_ShouldHandleNull()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            RecentOrders = null
        };

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.Null(customer.RecentOrders);
    }

    [Fact]
    public void MapFrom_WithSimpleList_ShouldCopyDirectly()
    {
        // Arrange
        var dto = new CustomerDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Tags = new List<string> { "VIP", "Premium", "Early Adopter" }
        };

        // Act
        var customer = Customer.MapFrom(dto);

        // Assert
        Assert.NotNull(customer.Tags);
        Assert.Equal(3, customer.Tags.Count);
        Assert.Contains("VIP", customer.Tags);
        Assert.Contains("Premium", customer.Tags);
        Assert.Contains("Early Adopter", customer.Tags);
    }

    [Fact]
    public void MapTo_WithListOfNestedObjects_ShouldMapAllItems()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            RecentOrders = new List<OrderItem>
            {
                new() { ProductId = 1, ProductName = "Laptop", Quantity = 1, UnitPrice = 999.99m },
                new() { ProductId = 2, ProductName = "Mouse", Quantity = 2, UnitPrice = 29.99m }
            }
        };

        // Act
        var dto = customer.MapTo();

        // Assert
        Assert.NotNull(dto.RecentOrders);
        Assert.Equal(2, dto.RecentOrders.Count);
        Assert.Equal("Laptop", dto.RecentOrders[0].ProductName);
        Assert.Equal("Mouse", dto.RecentOrders[1].ProductName);
    }

    [Fact]
    public void MapTo_WithSimpleList_ShouldCopyDirectly()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Tags = new List<string> { "Active", "Verified" }
        };

        // Act
        var dto = customer.MapTo();

        // Assert
        Assert.NotNull(dto.Tags);
        Assert.Equal(2, dto.Tags.Count);
        Assert.Contains("Active", dto.Tags);
        Assert.Contains("Verified", dto.Tags);
    }

    [Fact]
    public void MapFromList_ShouldMapEntireCollection()
    {
        // Arrange
        var dtos = new List<PersonDto>
        {
            new() { FirstName = "John", LastName = "Doe", Age = 30, Email = "john@example.com" },
            new() { FirstName = "Jane", LastName = "Smith", Age = 25, Email = "jane@example.com" },
            new() { FirstName = "Bob", LastName = "Johnson", Age = 35, Email = "bob@example.com" }
        };

        // Act
        var persons = Person.MapFrom(dtos);

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(3, persons.Count);
        Assert.Equal("John", persons[0].FirstName);
        Assert.Equal("Jane", persons[1].FirstName);
        Assert.Equal("Bob", persons[2].FirstName);
    }

    [Fact]
    public void MapFromList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var dtos = new List<PersonDto>();

        // Act
        var persons = Person.MapFrom(dtos);

        // Assert
        Assert.NotNull(persons);
        Assert.Empty(persons);
    }

    [Fact]
    public void MapFromList_WithNull_ShouldReturnEmptyList()
    {
        // Arrange
        IEnumerable<PersonDto>? dtos = null;

        // Act
        var persons = Person.MapFrom(dtos);

        // Assert
        Assert.NotNull(persons);
        Assert.Empty(persons);
    }

    [Fact]
    public void MapToList_ShouldMapEntireCollection()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { FirstName = "John", LastName = "Doe", Age = 30, Email = "john@example.com" },
            new() { FirstName = "Jane", LastName = "Smith", Age = 25, Email = "jane@example.com" }
        };

        // Act
        var dtos = Person.MapTo(persons);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(2, dtos.Count);
        Assert.Equal("John", dtos[0].FirstName);
        Assert.Equal("Jane", dtos[1].FirstName);
    }

    [Fact]
    public void ListMapping_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalDto = new CustomerDto
        {
            Id = 1,
            Name = "Test Customer",
            Email = "test@example.com",
            RecentOrders = new List<OrderItemDto>
            {
                new() { ProductId = 100, ProductName = "Test Product", Quantity = 5, UnitPrice = 50.00m }
            },
            Tags = new List<string> { "Tag1", "Tag2" }
        };

        // Act
        var customer = Customer.MapFrom(originalDto);
        var resultDto = customer.MapTo();

        // Assert
        Assert.Equal(originalDto.Id, resultDto.Id);
        Assert.NotNull(resultDto.RecentOrders);
        Assert.Single(resultDto.RecentOrders);
        Assert.Equal(originalDto.RecentOrders[0].ProductName, resultDto.RecentOrders[0].ProductName);
        Assert.NotNull(resultDto.Tags);
        Assert.Equal(2, resultDto.Tags.Count);
    }
}
