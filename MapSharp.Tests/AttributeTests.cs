using MapSharp.Tests.Models;
using System.Reflection;

namespace MapSharp.Tests;

public class AttributeTests
{
    [Fact]
    public void Person_ShouldHaveMapFromAttribute()
    {
        // Arrange
        var type = typeof(Person);

        // Act
        var attributes = type.GetCustomAttributes<MapFromAttribute>();

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Contains(attributes, a => a.SourceType == typeof(PersonDto));
    }

    [Fact]
    public void Person_ShouldHaveMapToAttribute()
    {
        // Arrange
        var type = typeof(Person);

        // Act
        var attributes = type.GetCustomAttributes<MapToAttribute>();

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Contains(attributes, a => a.TargetType == typeof(PersonDto));
    }

    [Fact]
    public void UserProfile_NameProperty_ShouldHaveMapPropertyAttribute()
    {
        // Arrange
        var property = typeof(UserProfile).GetProperty("Name");

        // Act
        var attribute = property?.GetCustomAttribute<MapPropertyAttribute>();

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("Username", attribute.SourcePropertyName);
    }

    [Fact]
    public void UserProfile_CreatedAtProperty_ShouldHaveIgnoreAttribute()
    {
        // Arrange
        var property = typeof(UserProfile).GetProperty("CreatedAt");

        // Act
        var attribute = property?.GetCustomAttribute<MapPropertyAttribute>();

        // Assert
        Assert.NotNull(attribute);
        Assert.True(attribute.Ignore);
    }

    [Fact]
    public void Product_ShouldHaveMapToAttribute()
    {
        // Arrange
        var type = typeof(Product);

        // Act
        var attributes = type.GetCustomAttributes<MapToAttribute>();

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Contains(attributes, a => a.TargetType == typeof(ProductDto));
    }

    [Fact]
    public void UserProfile_ShouldHaveMapFromAttribute()
    {
        // Arrange
        var type = typeof(UserProfile);

        // Act
        var attributes = type.GetCustomAttributes<MapFromAttribute>();

        // Assert
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
        Assert.Contains(attributes, a => a.SourceType == typeof(UserDto));
    }
}

