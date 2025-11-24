namespace MapSharp.Tests.Models;

[MapFrom(typeof(PersonDto))]
[MapTo(typeof(PersonDto))]
public partial class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

