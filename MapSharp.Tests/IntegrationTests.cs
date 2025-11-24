using MapSharp.Tests.Models;

namespace MapSharp.Tests;

public class IntegrationTests
{
    [Fact]
    public void RoundTrip_MapFromAndMapTo_ShouldPreserveData()
    {
        // Arrange
        var originalDto = new PersonDto
        {
            FirstName = "Charlie",
            LastName = "Davis",
            Age = 28,
            Email = "charlie.davis@example.com",
            PhoneNumber = "555-9876"
        };

        // Act
        var person = Person.MapFrom(originalDto);
        var resultDto = person.MapTo();

        // Assert
        Assert.Equal(originalDto.FirstName, resultDto.FirstName);
        Assert.Equal(originalDto.LastName, resultDto.LastName);
        Assert.Equal(originalDto.Age, resultDto.Age);
        Assert.Equal(originalDto.Email, resultDto.Email);
        Assert.Equal(originalDto.PhoneNumber, resultDto.PhoneNumber);
    }

    [Fact]
    public void MultipleMapping_WithDifferentInstances_ShouldWorkIndependently()
    {
        // Arrange
        var dto1 = new PersonDto
        {
            FirstName = "Person1",
            LastName = "Test1",
            Age = 20,
            Email = "person1@test.com",
            PhoneNumber = "111-1111"
        };

        var dto2 = new PersonDto
        {
            FirstName = "Person2",
            LastName = "Test2",
            Age = 30,
            Email = "person2@test.com",
            PhoneNumber = "222-2222"
        };

        // Act
        var person1 = Person.MapFrom(dto1);
        var person2 = Person.MapFrom(dto2);

        // Assert
        Assert.Equal("Person1", person1.FirstName);
        Assert.Equal("Person2", person2.FirstName);
        Assert.Equal(20, person1.Age);
        Assert.Equal(30, person2.Age);
    }

    [Fact]
    public void Mapping_WithEmptyStrings_ShouldHandleCorrectly()
    {
        // Arrange
        var dto = new PersonDto
        {
            FirstName = "",
            LastName = "",
            Age = 0,
            Email = "",
            PhoneNumber = ""
        };

        // Act
        var person = Person.MapFrom(dto);

        // Assert
        Assert.Equal("", person.FirstName);
        Assert.Equal("", person.LastName);
        Assert.Equal(0, person.Age);
    }

    [Fact]
    public void Mapping_WithSpecialCharacters_ShouldPreserveData()
    {
        // Arrange
        var dto = new PersonDto
        {
            FirstName = "José",
            LastName = "O'Brien-Smith",
            Age = 45,
            Email = "josé.o'brien@example.com",
            PhoneNumber = "+1 (555) 123-4567"
        };

        // Act
        var person = Person.MapFrom(dto);

        // Assert
        Assert.Equal("José", person.FirstName);
        Assert.Equal("O'Brien-Smith", person.LastName);
        Assert.Equal("+1 (555) 123-4567", person.PhoneNumber);
    }

    [Theory]
    [InlineData("John", "Doe", 25)]
    [InlineData("Jane", "Smith", 30)]
    [InlineData("Bob", "Johnson", 40)]
    public void MapFrom_WithVariousData_ShouldMapCorrectly(string firstName, string lastName, int age)
    {
        // Arrange
        var dto = new PersonDto
        {
            FirstName = firstName,
            LastName = lastName,
            Age = age,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com"
        };

        // Act
        var person = Person.MapFrom(dto);

        // Assert
        Assert.Equal(firstName, person.FirstName);
        Assert.Equal(lastName, person.LastName);
        Assert.Equal(age, person.Age);
    }
}

