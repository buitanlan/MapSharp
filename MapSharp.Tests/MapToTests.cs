using MapSharp.Tests.Models;

namespace MapSharp.Tests;

public class MapToTests
{
    [Fact]
    public void MapTo_WithValidPerson_ShouldMapAllProperties()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Age = 35,
            Email = "alice.johnson@example.com",
            PhoneNumber = "555-1234"
        };

        // Act
        var dto = person.MapTo();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(person.FirstName, dto.FirstName);
        Assert.Equal(person.LastName, dto.LastName);
        Assert.Equal(person.Age, dto.Age);
        Assert.Equal(person.Email, dto.Email);
        Assert.Equal(person.PhoneNumber, dto.PhoneNumber);
    }

    [Fact]
    public void MapTo_WithProduct_ShouldMapAllProperties()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 999.99m,
            Description = "High-performance laptop"
        };

        // Act
        var dto = product.MapTo();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(product.Id, dto.Id);
        Assert.Equal(product.Name, dto.Name);
        Assert.Equal(product.Price, dto.Price);
        Assert.Equal(product.Description, dto.Description);
    }

    [Fact]
    public void MapTo_WithNullableValues_ShouldHandleCorrectly()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "Bob",
            LastName = "Brown",
            Age = 40,
            Email = "bob@example.com",
            PhoneNumber = null
        };

        // Act
        var dto = person.MapTo();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.PhoneNumber);
    }

    [Fact]
    public void MapTo_WithDecimalValues_ShouldPreservePrecision()
    {
        // Arrange
        var product = new Product
        {
            Id = 2,
            Name = "Keyboard",
            Price = 49.95m,
            Description = "Mechanical keyboard"
        };

        // Act
        var dto = product.MapTo();

        // Assert
        Assert.Equal(49.95m, dto.Price);
    }
}

