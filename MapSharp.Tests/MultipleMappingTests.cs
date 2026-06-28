using MapSharp.Tests.Models;

namespace MapSharp.Tests;

public class MultipleMappingTests
{
    [Fact]
    public void MapFrom_WithMultipleSourceTypes_ShouldMapFromPersonDto()
    {
        var dto = new PersonDto
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Email = "john@example.com"
        };

        var person = MultiMapPerson.MapFrom(dto);

        Assert.Equal(dto.FirstName, person.FirstName);
        Assert.Equal(dto.LastName, person.LastName);
        Assert.Equal(dto.Age, person.Age);
        Assert.Equal(dto.Email, person.Email);
    }

    [Fact]
    public void MapFrom_WithMultipleSourceTypes_ShouldMapFromPersonViewModel()
    {
        var viewModel = new PersonViewModel
        {
            FirstName = "Jane",
            LastName = "Smith",
            Age = 25
        };

        var person = MultiMapPerson.MapFrom(viewModel);

        Assert.Equal(viewModel.FirstName, person.FirstName);
        Assert.Equal(viewModel.LastName, person.LastName);
        Assert.Equal(viewModel.Age, person.Age);
    }

    [Fact]
    public void MapTo_WithMultipleTargetTypes_ShouldMapToPersonDto()
    {
        var person = new MultiMapPerson
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Age = 35,
            Email = "alice@example.com"
        };

        var dto = person.MapToPersonDto();

        Assert.Equal(person.FirstName, dto.FirstName);
        Assert.Equal(person.LastName, dto.LastName);
        Assert.Equal(person.Age, dto.Age);
        Assert.Equal(person.Email, dto.Email);
    }

    [Fact]
    public void MapTo_WithMultipleTargetTypes_ShouldMapToPersonSummaryDto()
    {
        var person = new MultiMapPerson
        {
            FirstName = "Bob",
            LastName = "Brown",
            Age = 40,
            Email = "bob@example.com"
        };

        var summary = person.MapToPersonSummaryDto();

        Assert.Equal(person.FirstName, summary.FirstName);
        Assert.Equal(person.LastName, summary.LastName);
        Assert.Equal(person.Email, summary.Email);
    }

    [Fact]
    public void MapToPersonDto_WithCollection_ShouldMapAllItems()
    {
        var persons = new List<MultiMapPerson>
        {
            new() { FirstName = "A", LastName = "One", Age = 1, Email = "a@example.com" },
            new() { FirstName = "B", LastName = "Two", Age = 2, Email = "b@example.com" }
        };

        var dtos = MultiMapPerson.MapToPersonDto(persons);

        Assert.Equal(2, dtos.Count);
        Assert.Equal("A", dtos[0].FirstName);
        Assert.Equal("B", dtos[1].FirstName);
    }
}
