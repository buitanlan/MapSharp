using MapSharp.Tests.Models;

namespace MapSharp.Tests;

public class MapFromTests
{
    [Fact]
    public void MapFrom_WithValidDto_ShouldMapAllProperties()
    {
        // Arrange
        var dto = new PersonDto
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Email = "john.doe@example.com",
            PhoneNumber = "123-456-7890"
        };

        // Act
        var person = Person.MapFrom(dto);

        // Assert
        Assert.NotNull(person);
        Assert.Equal(dto.FirstName, person.FirstName);
        Assert.Equal(dto.LastName, person.LastName);
        Assert.Equal(dto.Age, person.Age);
        Assert.Equal(dto.Email, person.Email);
        Assert.Equal(dto.PhoneNumber, person.PhoneNumber);
    }

    [Fact]
    public void MapFrom_WithNullableProperties_ShouldMapCorrectly()
    {
        // Arrange
        var dto = new PersonDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Age = 25,
            Email = "jane.smith@example.com",
            PhoneNumber = null
        };

        // Act
        var person = Person.MapFrom(dto);

        // Assert
        Assert.NotNull(person);
        Assert.Equal(dto.FirstName, person.FirstName);
        Assert.Null(person.PhoneNumber);
    }

    [Fact]
    public void MapFrom_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        PersonDto? dto = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Person.MapFrom(dto!));
    }

    [Fact]
    public void MapFrom_WithCustomPropertyMapping_ShouldMapCorrectly()
    {
        // Arrange
        var userDto = new UserDto
        {
            Username = "johndoe",
            Email = "john@example.com",
            IsActive = true
        };

        // Act
        var userProfile = UserProfile.MapFrom(userDto);

        // Assert
        Assert.NotNull(userProfile);
        Assert.Equal(userDto.Username, userProfile.Name);
        Assert.Equal(userDto.Email, userProfile.Email);
        Assert.Equal(userDto.IsActive, userProfile.IsActive);
    }

    [Fact]
    public void MapFrom_WithIgnoredProperty_ShouldNotMapIgnoredProperty()
    {
        // Arrange
        var userDto = new UserDto
        {
            Username = "johndoe",
            Email = "john@example.com",
            IsActive = true
        };

        var beforeMapping = DateTime.UtcNow;

        // Act
        var userProfile = UserProfile.MapFrom(userDto);

        // Assert
        Assert.NotNull(userProfile);
        // CreatedAt should be default value (not mapped from source)
        Assert.True(userProfile.CreatedAt >= beforeMapping);
    }
}

